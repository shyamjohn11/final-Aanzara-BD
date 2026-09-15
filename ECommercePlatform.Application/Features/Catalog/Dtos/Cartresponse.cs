namespace ECommercePlatform.Application.Features.Cart.Dtos;

public sealed record CartResponse
{
    /// <summary>Null when the caller has no cart yet — an empty cart, not an error.</summary>
    public Guid? CartId { get; init; }

    public IReadOnlyCollection<CartItemResponse> Items { get; init; } = [];

    public CartSummaryResponse Summary { get; init; } = new();
}

public sealed record CartItemResponse
{
    public Guid CartItemId { get; init; }

    public Guid ProductId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public string Sku { get; init; } = string.Empty;

    public decimal UnitPrice { get; init; }

    public decimal Mrp { get; init; }

    public int Quantity { get; init; }

    /// <summary>Pre-tax line amount (UnitPrice × Quantity) — the base amount GST is charged on.</summary>
    public decimal LineTotal { get; init; }

    /// <summary>
    /// GST rate for this line, resolved from the product's category TaxRule with the
    /// category-less rule as default; 0 when the product is GST-free; 18% (the
    /// module default) when no TaxRule is configured at all.
    /// </summary>
    public decimal GstPercentage { get; init; }

    /// <summary>LineTotal × GstPercentage / 100, rounded to 2 decimals.</summary>
    public decimal GstAmount { get; init; }

    /// <summary>LineTotal + GstAmount — the line's payable amount before delivery, handling and any coupon.</summary>
    public decimal ItemTotal { get; init; }

    /// <summary>Total stock across all warehouses, less what other orders already hold.</summary>
    public int AvailableQuantity { get; init; }

    public bool InStock { get; init; }
}

public sealed record CartSummaryResponse
{
    public int ItemCount { get; init; }

    public decimal Subtotal { get; init; }

    public decimal TaxTotal { get; init; }

    public decimal DeliveryCharge { get; init; }

    public decimal HandlingFee { get; init; }

    /// <summary>Only reflects the applied coupon if it is still valid right now; an expired or unmet coupon discounts nothing.</summary>
    public decimal DiscountTotal { get; init; }

    public decimal Total { get; init; }

    /// <summary>Echoes the code so the client can show what's applied — null if none is, or the applied one no longer qualifies.</summary>
    public string? AppliedCouponCode { get; init; }
}