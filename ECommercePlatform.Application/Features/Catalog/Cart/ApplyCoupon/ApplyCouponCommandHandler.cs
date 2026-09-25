using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Common;
using ECommercePlatform.Application.Features.Admin.Coupons;
using ECommercePlatform.Application.Features.Cart.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Enums;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Cart.ApplyCoupon;

/// <summary>
/// Attaches a shopper-entered coupon code to the caller's cart. Codes are the
/// admin-managed marketing coupons (same source as GET /api/v1/deals/coupons);
/// when a code exists only in the admin store it is materialized into the
/// Coupons table so the cart FK and PlaceOrder pricing both see it.
/// </summary>
public sealed class ApplyCouponCommandHandler
    : ICommandHandler<ApplyCouponCommand, Result<CartSummaryResponse>>
{
    private readonly ICartRepository _carts;
    private readonly ICouponRepository _coupons;
    private readonly ITaxRuleRepository _taxRules;
    private readonly IDeliveryRuleRepository _deliveryRules;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public ApplyCouponCommandHandler(
        ICartRepository carts,
        ICouponRepository coupons,
        ITaxRuleRepository taxRules,
        IDeliveryRuleRepository deliveryRules,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _carts = carts;
        _coupons = coupons;
        _taxRules = taxRules;
        _deliveryRules = deliveryRules;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<CartSummaryResponse>> Handle(
        ApplyCouponCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code?.Trim().ToUpperInvariant() ?? string.Empty;
        if (code.Length == 0)
        {
            return Result.Failure<CartSummaryResponse>(
                Error.Validation("cart.coupon_required", "Please enter a coupon code."));
        }

        var cart = await _carts.GetByUserIdAsync(request.UserId, cancellationToken);
        if (cart is null || cart.CartItems.Count == 0)
        {
            return Result.Failure<CartSummaryResponse>(CartErrors.EmptyCart);
        }

        var coupon = await ResolveCouponAsync(code, cancellationToken);
        if (coupon is null)
        {
            return Result.Failure<CartSummaryResponse>(CartErrors.CouponNotFound);
        }

        var now = _timeProvider.GetUtcNow();
        var utcNow = now.UtcDateTime;

        if (coupon.Status != CouponStatus.Active)
        {
            return Result.Failure<CartSummaryResponse>(CartErrors.CouponInactive);
        }

        if (utcNow < coupon.StartDate || utcNow > coupon.EndDate)
        {
            return Result.Failure<CartSummaryResponse>(CartErrors.CouponExpired);
        }

        var subtotal = cart.CartItems.Sum(i => i.Quantity * i.Product.Price);
        if (subtotal < coupon.MinOrderValue)
        {
            return Result.Failure<CartSummaryResponse>(
                Error.Validation(
                    "cart.coupon_min_order",
                    $"This coupon requires a minimum order of ₹{coupon.MinOrderValue:0}."));
        }

        cart.AppliedCouponId = coupon.CouponId;
        cart.AppliedCoupon = coupon;
        cart.UpdatedAt = now;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var lines = cart.CartItems
            .Select(i => new CartLine(i.Product, i.Quantity))
            .ToArray();

        var summary = CartPricing.Summarize(
            lines,
            await _taxRules.GetAllAsync(cancellationToken),
            await _deliveryRules.GetAsync(cancellationToken),
            coupon,
            now);

        if (summary.DiscountTotal <= 0 || summary.AppliedCouponCode is null)
        {
            // Pricing rejected the coupon (edge: min order raced with edits).
            cart.AppliedCouponId = null;
            cart.AppliedCoupon = null;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<CartSummaryResponse>(CartErrors.CouponNotApplicable);
        }

        return Result.Success(summary);
    }

    /// <summary>
    /// Admin marketing store is the source of truth for shopper-facing codes;
    /// the SQL Coupons row is the FK target for cart/order pricing.
    /// </summary>
    private async Task<Coupon?> ResolveCouponAsync(string code, CancellationToken cancellationToken)
    {
        CouponAdminSeeds.Ensure();

        var admin = AdminCrudStore<CouponAdminResponse>.All()
            .FirstOrDefault(c =>
                string.Equals(c.Code, code, StringComparison.OrdinalIgnoreCase));

        var db = await _coupons.GetByCodeAsync(code, cancellationToken);

        if (admin is not null)
        {
            if (!string.Equals(admin.Status, "Active", StringComparison.OrdinalIgnoreCase))
            {
                if (db is not null && db.Status == CouponStatus.Active)
                {
                    db.Status = CouponStatus.Inactive;
                    db.UpdatedAt = DateTimeOffset.UtcNow;
                }

                return null;
            }

            if (db is null)
            {
                db = Materialize(admin);
                _coupons.Add(db);
            }
            else
            {
                ApplyAdminFields(db, admin);
            }

            return db;
        }

        return db;
    }

    private static Coupon Materialize(CouponAdminResponse admin)
    {
        var now = DateTimeOffset.UtcNow;
        var coupon = new Coupon
        {
            CouponId = admin.Id == Guid.Empty ? Guid.NewGuid() : admin.Id,
            CreatedAt = admin.CreatedAt == default ? now : admin.CreatedAt,
        };
        ApplyAdminFields(coupon, admin);
        return coupon;
    }

    private static void ApplyAdminFields(Coupon coupon, CouponAdminResponse admin)
    {
        var now = DateTimeOffset.UtcNow;
        coupon.CouponCode = admin.Code.Trim().ToUpperInvariant();
        coupon.DiscountType = ParseDiscountType(admin.DiscountType);
        coupon.DiscountValue = admin.DiscountValue;
        coupon.MinOrderValue = admin.MinOrderValue;
        coupon.UsageLimitPerUser = admin.UsageLimit <= 0 ? 1 : admin.UsageLimit;
        coupon.StartDate = (admin.StartsAt ?? now.AddDays(-1)).UtcDateTime;
        coupon.EndDate = (admin.EndsAt ?? now.AddDays(30)).UtcDateTime;
        coupon.Status = CouponStatus.Active;
        coupon.UpdatedAt = now;
    }

    private static DiscountType ParseDiscountType(string? value)
        => value?.Trim().ToLowerInvariant() switch
        {
            "flat" => DiscountType.flat,
            _ => DiscountType.percentage,
        };
}

public sealed class RemoveCouponCommandHandler
    : ICommandHandler<RemoveCouponCommand, Result<CartSummaryResponse>>
{
    private readonly ICartRepository _carts;
    private readonly ITaxRuleRepository _taxRules;
    private readonly IDeliveryRuleRepository _deliveryRules;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public RemoveCouponCommandHandler(
        ICartRepository carts,
        ITaxRuleRepository taxRules,
        IDeliveryRuleRepository deliveryRules,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _carts = carts;
        _taxRules = taxRules;
        _deliveryRules = deliveryRules;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<CartSummaryResponse>> Handle(
        RemoveCouponCommand request, CancellationToken cancellationToken)
    {
        var cart = await _carts.GetByUserIdAsync(request.UserId, cancellationToken);
        if (cart is null)
        {
            return Result.Success(new CartSummaryResponse());
        }

        if (cart.AppliedCouponId is not null || cart.AppliedCoupon is not null)
        {
            cart.AppliedCouponId = null;
            cart.AppliedCoupon = null;
            cart.UpdatedAt = _timeProvider.GetUtcNow();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var lines = cart.CartItems
            .Select(i => new CartLine(i.Product, i.Quantity))
            .ToArray();

        var summary = CartPricing.Summarize(
            lines,
            await _taxRules.GetAllAsync(cancellationToken),
            await _deliveryRules.GetAsync(cancellationToken),
            cart.AppliedCoupon,
            _timeProvider.GetUtcNow());

        return Result.Success(summary);
    }
}
