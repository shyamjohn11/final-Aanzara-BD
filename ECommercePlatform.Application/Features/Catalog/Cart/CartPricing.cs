using ECommercePlatform.Application.Features.Cart.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Enums;

namespace ECommercePlatform.Application.Features.Cart;

/// <summary>
/// Turns a cart's line items plus the current tax/delivery/coupon configuration
/// into the numbers a client renders. Kept separate from the query handler so the
/// handler stays a fetch-and-assemble orchestrator and this stays independently
/// testable without a database.
/// </summary>
public static class CartPricing
{
    public static CartSummaryResponse Summarize(
        IReadOnlyCollection<CartLine> lines,
        IReadOnlyCollection<TaxRule> taxRules,
        DeliveryRule? deliveryRule,
        Domain.Entities.Coupon? appliedCoupon,
        DateTimeOffset now)
    {
        var subtotal = lines.Sum(l => l.Quantity * l.Product.Price);

        var taxTotal = lines.Sum(line => Tax(line, taxRules));

        var (deliveryCharge, handlingFee) = Delivery(subtotal, deliveryRule);

        var discount = Discount(subtotal, appliedCoupon, now);

        var total = subtotal + taxTotal + deliveryCharge + handlingFee - discount;

        return new CartSummaryResponse
        {
            ItemCount = lines.Sum(l => l.Quantity),
            Subtotal = subtotal,
            TaxTotal = taxTotal,
            DeliveryCharge = deliveryCharge,
            HandlingFee = handlingFee,
            DiscountTotal = discount,
            Total = total < 0 ? 0 : total,
            AppliedCouponCode = discount > 0 ? appliedCoupon!.CouponCode : null
        };
    }

    /// <summary>
    /// The rate charged when no TaxRule is configured for a product at all. The
    /// TaxRules table has no management surface yet, so until rules are seeded
    /// every taxable line uses this instead of silently pricing at 0%.
    /// </summary>
    public const decimal DefaultGstPercentage = 18m;

    /// <summary>
    /// The GST rate that applies to a product: 0 when it is GST-free, otherwise the
    /// rule for the product's own category falling back to the one category-less
    /// default rule, else <see cref="DefaultGstPercentage"/> when no rule exists.
    /// Shared by the per-item mapping and the cart summary so the two can never
    /// disagree on a rate.
    /// </summary>
    public static decimal GstPercentageFor(Product product, IReadOnlyCollection<TaxRule> taxRules)
    {
        if (product.IsGstFree)
        {
            return 0m;
        }

        var rule = taxRules.FirstOrDefault(r => r.CategoryId == product.CategoryId)
            ?? taxRules.FirstOrDefault(r => r.CategoryId is null);

        return rule?.GstPercentage ?? DefaultGstPercentage;
    }

    private static decimal Tax(CartLine line, IReadOnlyCollection<TaxRule> taxRules)
    {
        var lineSubtotal = line.Quantity * line.Product.Price;

        return Math.Round(lineSubtotal * GstPercentageFor(line.Product, taxRules) / 100m, 2);
    }

    private static (decimal DeliveryCharge, decimal HandlingFee) Delivery(
        decimal subtotal, DeliveryRule? rule)
    {
        if (rule is null)
        {
            return (0m, 0m);
        }

        var deliveryCharge = subtotal >= rule.MinOrderValueForFreeDelivery
            ? 0m
            : rule.FlatDeliveryCharge;

        return (deliveryCharge, rule.HandlingFee);
    }

    private static decimal Discount(decimal subtotal, Domain.Entities.Coupon? coupon, DateTimeOffset now)
    {
        // Coupon.StartDate/EndDate predate the rest of the schema's move to
        // DateTimeOffset and are still plain DateTime, stored as UTC by
        // convention — compare against the UTC half of "now" rather than widen
        // them, so this doesn't silently assume the caller's local offset.
        var utcNow = now.UtcDateTime;

        if (coupon is null
            || coupon.Status != CouponStatus.Active
            || utcNow < coupon.StartDate
            || utcNow > coupon.EndDate
            || subtotal < coupon.MinOrderValue)
        {
            return 0m;
        }

        var discount = coupon.DiscountType == DiscountType.percentage
            ? subtotal * coupon.DiscountValue / 100m
            : coupon.DiscountValue;

        // A coupon can shave the order down, never below it.
        return Math.Min(discount, subtotal);
    }
}

/// <summary>One priced cart line: a product and the quantity of it in the cart.</summary>
public sealed record CartLine(Domain.Entities.Product Product, int Quantity);