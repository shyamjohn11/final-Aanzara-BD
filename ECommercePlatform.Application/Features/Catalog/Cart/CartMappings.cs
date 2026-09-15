using ECommercePlatform.Application.Features.Cart.Dtos;
using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Features.Cart;

public static class CartMappings
{
    /// <summary>
    /// Maps one cart line with its GST breakdown. The rate comes from the caller's
    /// already-loaded TaxRules rather than a fresh query, and the amounts use the
    /// same rounding as CartPricing.Summarize, so the item totals and the cart
    /// summary always reconcile.
    /// </summary>
    public static CartItemResponse ToResponse(
        this CartItem item, int availableQuantity, IReadOnlyCollection<TaxRule> taxRules)
    {
        var baseAmount = item.Quantity * item.Product.Price;

        var gstPercentage = CartPricing.GstPercentageFor(item.Product, taxRules);
        var gstAmount = Math.Round(baseAmount * gstPercentage / 100m, 2);

        return new()
        {
            CartItemId = item.CartItemId,
            ProductId = item.ProductId,
            ProductName = item.Product.ProductName,
            Sku = item.Product.Sku,
            UnitPrice = item.Product.Price,
            Mrp = item.Product.Mrp,
            Quantity = item.Quantity,
            LineTotal = baseAmount,
            GstPercentage = gstPercentage,
            GstAmount = gstAmount,
            ItemTotal = baseAmount + gstAmount,
            AvailableQuantity = availableQuantity,
            InStock = availableQuantity >= item.Quantity
        };
    }
}
