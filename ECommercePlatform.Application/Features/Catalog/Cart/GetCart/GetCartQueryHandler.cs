using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Cart.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Cart.GetCart;

public sealed class GetCartQueryHandler : IQueryHandler<GetCartQuery, Result<CartResponse>>
{
    private readonly ICartRepository _carts;
    private readonly IInventoryRepository _inventory;
    private readonly ITaxRuleRepository _taxRules;
    private readonly IDeliveryRuleRepository _deliveryRules;
    private readonly TimeProvider _timeProvider;

    public GetCartQueryHandler(
        ICartRepository carts,
        IInventoryRepository inventory,
        ITaxRuleRepository taxRules,
        IDeliveryRuleRepository deliveryRules,
        TimeProvider timeProvider)
    {
        _carts = carts;
        _inventory = inventory;
        _taxRules = taxRules;
        _deliveryRules = deliveryRules;
        _timeProvider = timeProvider;
    }

    public async Task<Result<CartResponse>> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await _carts.GetByUserIdAsync(request.UserId, cancellationToken);

        // No cart row yet is not a failure — it is an empty cart. Reading must
        // never create one; that stays the job of "add item".
        if (cart is null || cart.CartItems.Count == 0)
        {
            return Result.Success(new CartResponse
            {
                CartId = cart?.CartId,
                Items = [],
                Summary = new CartSummaryResponse()
            });
        }

        var productIds = cart.CartItems.Select(i => i.ProductId).Distinct().ToArray();

        var availableByProduct = (await _inventory.GetByProductIdsAsync(productIds, cancellationToken))
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.StockQuantity - i.ReservedQuantity));

        var taxRules = await _taxRules.GetAllAsync(cancellationToken);

        var items = cart.CartItems
            .Select(i => i.ToResponse(availableByProduct.GetValueOrDefault(i.ProductId), taxRules))
            .ToArray();

        var deliveryRule = await _deliveryRules.GetAsync(cancellationToken);

        var lines = cart.CartItems
            .Select(i => new CartLine(i.Product, i.Quantity))
            .ToArray();

        var summary = CartPricing.Summarize(
            lines, taxRules, deliveryRule, cart.AppliedCoupon, _timeProvider.GetUtcNow());

        return Result.Success(new CartResponse
        {
            CartId = cart.CartId,
            Items = items,
            Summary = summary
        });
    }
}