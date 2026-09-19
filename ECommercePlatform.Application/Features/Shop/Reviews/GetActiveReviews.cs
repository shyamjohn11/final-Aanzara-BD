using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Common;
using ECommercePlatform.Application.Features.Admin.Reviews;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Shop.Reviews;

/// <summary>Public storefront reviews — approved only, newest first. No auth.</summary>
public sealed record GetActiveReviewsQuery(
    string? ProductName = null,
    int Count = 25) : IQuery<Result<IReadOnlyList<ReviewResponse>>>;

public sealed class GetActiveReviewsQueryHandler(
    IAdminRepository<Review> reviews) : IQueryHandler<GetActiveReviewsQuery, Result<IReadOnlyList<ReviewResponse>>>
{
    public async Task<Result<IReadOnlyList<ReviewResponse>>> Handle(
        GetActiveReviewsQuery request, CancellationToken cancellationToken)
    {
        var count = Math.Clamp(request.Count <= 0 ? 25 : request.Count, 1, 100);
        var filter = AdminFilters.True<Review>().And(r => r.Status == "Approved");
        if (!string.IsNullOrWhiteSpace(request.ProductName))
        {
            var pn = request.ProductName.Trim();
            filter = filter.And(r => r.ProductName.Contains(pn));
        }
        var items = await reviews.PageAsync(filter, q => q.OrderByDescending(r => r.CreatedAt), 0, count, cancellationToken);
        return Result.Success<IReadOnlyList<ReviewResponse>>(items.Select(r => r.ToDto()).ToArray());
    }
}
