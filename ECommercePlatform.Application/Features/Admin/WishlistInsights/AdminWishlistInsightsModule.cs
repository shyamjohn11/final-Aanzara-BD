using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Common;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.WishlistInsights;

// ID #147 — GET /api/admin/wishlist-insights (read-only aggregates).

public sealed record WishlistInsightResponse : IAdminEntity
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int WishlistCount { get; init; }
    public bool InStock { get; init; } = true;
}

public sealed record GetWishlistInsightsQuery : IQuery<Result<PagedResult<WishlistInsightResponse>>>
{
    public string? Search { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 25;
}

internal static class WishlistInsightSeeds
{
    internal static void Ensure()
    {
        AdminCrudStore<WishlistInsightResponse>.EnsureSeeded(() =>
        {
            WishlistInsightResponse Row(string name, int count, bool inStock) => new()
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductName = name,
                WishlistCount = count,
                InStock = inStock,
            };

            return new List<WishlistInsightResponse>
            {
                Row("Basmati Rice 5kg", 214, true),
                Row("Sunflower Oil 5L", 187, true),
                Row("Aashirvaad Atta 10kg", 165, false),
                Row("Organic Honey 500g", 142, true),
            };
        });
    }
}

public sealed class GetWishlistInsightsQueryHandler
    : IQueryHandler<GetWishlistInsightsQuery, Result<PagedResult<WishlistInsightResponse>>>
{
    public Task<Result<PagedResult<WishlistInsightResponse>>> Handle(
        GetWishlistInsightsQuery request, CancellationToken cancellationToken)
    {
        WishlistInsightSeeds.Ensure();
        var filtered = AdminCrudStore<WishlistInsightResponse>.All()
            .Where(w => AdminPaging.Matches(request.Search, w.ProductName))
            .OrderByDescending(w => w.WishlistCount);

        return Task.FromResult(Result.Success(AdminPaging.ToPaged(filtered, request.Page, request.PageSize)));
    }
}
