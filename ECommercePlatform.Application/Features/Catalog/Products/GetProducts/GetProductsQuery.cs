using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Products.GetProducts;

/// <summary>
/// Product search. Filtering by CategoryId or SubCategoryId is how a client walks
/// down from the product tree into the leaves.
/// </summary>
public sealed record GetProductsQuery : IQuery<Result<PagedResult<ProductSummaryResponse>>>
{
    public Guid? CategoryId { get; init; }

    public Guid? SubCategoryId { get; init; }

    public Guid? BrandId { get; init; }

    [MaxLength(200)]
    public string? Search { get; init; }

    [MaxLength(20)]
    public string? Status { get; init; }

    [Range(0, 99999999.99)]
    public decimal? MinPrice { get; init; }

    [Range(0, 99999999.99)]
    public decimal? MaxPrice { get; init; }

    /// <summary>One of: sku, mrp, price, created. Unknown values fall back to name.</summary>
    [MaxLength(30)]
    public string? SortBy { get; init; }

    public bool SortDescending { get; init; }

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 200)]
    public int PageSize { get; init; } = 25;
}