using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Common;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.WholesalePricing;

// IDs #145-146 — GET price list + PUT bulk price update.

public sealed record WholesalePriceResponse : IAdminEntity
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public decimal BasePrice { get; init; }
    public decimal WholesalePrice { get; init; }
    public int MinQty { get; init; } = 1;
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record GetWholesalePricesQuery : IQuery<Result<PagedResult<WholesalePriceResponse>>>
{
    public string? Search { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 25;
}

public sealed record WholesalePriceUpdateDto
{
    public Guid ProductId { get; init; }
    [Range(0, 10000000)] public decimal WholesalePrice { get; init; }
    [Range(1, 1000000)] public int MinQty { get; init; } = 1;
}

public sealed record BulkUpdateWholesalePricesCommand(IReadOnlyList<WholesalePriceUpdateDto> Items)
    : ICommand<Result<IReadOnlyList<WholesalePriceResponse>>>;

internal static class WholesalePriceSeeds
{
    internal static void Ensure()
    {
        AdminCrudStore<WholesalePriceResponse>.EnsureSeeded(() =>
        {
            var now = DateTimeOffset.UtcNow;
            WholesalePriceResponse Row(string name, string sku, decimal basePrice, decimal wsPrice, int minQty) => new()
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductName = name,
                Sku = sku,
                BasePrice = basePrice,
                WholesalePrice = wsPrice,
                MinQty = minQty,
                UpdatedAt = now,
            };

            return new List<WholesalePriceResponse>
            {
                Row("Basmati Rice 5kg", "RICE-5KG", 649, 599, 10),
                Row("Sunflower Oil 5L", "OIL-5L", 725, 680, 6),
                Row("Aashirvaad Atta 10kg", "ATTA-10KG", 495, 460, 10),
                Row("Sugar 5kg", "SUGAR-5KG", 240, 220, 20),
            };
        });
    }
}

public sealed class GetWholesalePricesQueryHandler
    : IQueryHandler<GetWholesalePricesQuery, Result<PagedResult<WholesalePriceResponse>>>
{
    public Task<Result<PagedResult<WholesalePriceResponse>>> Handle(
        GetWholesalePricesQuery request, CancellationToken cancellationToken)
    {
        WholesalePriceSeeds.Ensure();
        var filtered = AdminCrudStore<WholesalePriceResponse>.All()
            .Where(p => AdminPaging.Matches(request.Search, p.ProductName, p.Sku))
            .OrderBy(p => p.ProductName);

        return Task.FromResult(Result.Success(AdminPaging.ToPaged(filtered, request.Page, request.PageSize)));
    }
}

public sealed class BulkUpdateWholesalePricesCommandHandler
    : IRequestHandler<BulkUpdateWholesalePricesCommand, Result<IReadOnlyList<WholesalePriceResponse>>>
{
    public Task<Result<IReadOnlyList<WholesalePriceResponse>>> Handle(
        BulkUpdateWholesalePricesCommand request, CancellationToken cancellationToken)
    {
        WholesalePriceSeeds.Ensure();
        if (request.Items is null || request.Items.Count == 0)
        {
            return Task.FromResult(Result.Failure<IReadOnlyList<WholesalePriceResponse>>(
                Error.Validation("admin.empty_bulk_update", "At least one price update is required.")));
        }

        var updated = new List<WholesalePriceResponse>();
        foreach (var item in request.Items)
        {
            var existing = AdminCrudStore<WholesalePriceResponse>.All()
                .FirstOrDefault(p => p.ProductId == item.ProductId || p.Id == item.ProductId);

            if (existing is null)
            {
                continue;
            }

            var next = existing with
            {
                WholesalePrice = item.WholesalePrice,
                MinQty = item.MinQty <= 0 ? existing.MinQty : item.MinQty,
                UpdatedAt = DateTimeOffset.UtcNow,
            };
            AdminCrudStore<WholesalePriceResponse>.Put(next);
            updated.Add(next);
        }

        return Task.FromResult(Result.Success<IReadOnlyList<WholesalePriceResponse>>(updated));
    }
}
