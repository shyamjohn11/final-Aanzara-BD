using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Banners;
using ECommercePlatform.Application.Features.Admin.CartRules;
using ECommercePlatform.Application.Features.Admin.Common;
using ECommercePlatform.Application.Features.Admin.Combos;
using ECommercePlatform.Application.Features.Admin.Coupons;
using ECommercePlatform.Application.Features.Admin.Offers;
using ECommercePlatform.Application.Features.Admin.WholesalePricing;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Shop.Deals;

/// <summary>
/// Public storefront reads over the admin-managed marketing tables.
/// Active-only, newest first. No new tables: offers/combos live in SQL
/// Server, coupons in the admin in-memory store.
///
public sealed record GetActiveOffersQuery(int Count = 8)
    : IQuery<Result<IReadOnlyList<OfferResponse>>>;

public sealed record GetActiveCombosQuery(int Count = 8)
    : IQuery<Result<IReadOnlyList<ComboResponse>>>;

public sealed record GetActiveCouponsQuery(int Count = 8)
    : IQuery<Result<IReadOnlyList<CouponAdminResponse>>>;

public sealed record GetActiveBannersQuery(int Count = 8)
    : IQuery<Result<IReadOnlyList<BannerResponse>>>;

public sealed record GetActiveCartRulesQuery(int Count = 8)
    : IQuery<Result<IReadOnlyList<CartRuleResponse>>>;

public sealed record GetActiveBulkTiersQuery(int Count = 8)
    : IQuery<Result<IReadOnlyList<WholesalePriceResponse>>>;

public sealed class GetActiveOffersQueryHandler
    : IQueryHandler<GetActiveOffersQuery, Result<IReadOnlyList<OfferResponse>>>
{
    private readonly IAdminRepository<Offer> _offers;

    public GetActiveOffersQueryHandler(IAdminRepository<Offer> offers) => _offers = offers;

    public async Task<Result<IReadOnlyList<OfferResponse>>> Handle(
        GetActiveOffersQuery request, CancellationToken cancellationToken)
    {
        var count = Math.Clamp(request.Count <= 0 ? 8 : request.Count, 1, 50);
        var items = await _offers.ListAsync(
            o => o.Status == "Active",
            q => q.OrderByDescending(o => o.CreatedAt),
            cancellationToken);

        return Result.Success<IReadOnlyList<OfferResponse>>(
            items.Select(o => o.ToDto()).Take(count).ToArray());
    }
}

public sealed class GetActiveCombosQueryHandler
    : IQueryHandler<GetActiveCombosQuery, Result<IReadOnlyList<ComboResponse>>>
{
    private readonly IAdminRepository<Combo> _combos;

    public GetActiveCombosQueryHandler(IAdminRepository<Combo> combos) => _combos = combos;

    public async Task<Result<IReadOnlyList<ComboResponse>>> Handle(
        GetActiveCombosQuery request, CancellationToken cancellationToken)
    {
        var count = Math.Clamp(request.Count <= 0 ? 8 : request.Count, 1, 50);
        var items = await _combos.ListAsync(
            c => c.Status == "Active",
            q => q.OrderByDescending(c => c.CreatedAt),
            cancellationToken);

        return Result.Success<IReadOnlyList<ComboResponse>>(
            items.Select(c => c.ToDto()).Take(count).ToArray());
    }
}

public sealed class GetActiveCouponsQueryHandler
    : IQueryHandler<GetActiveCouponsQuery, Result<IReadOnlyList<CouponAdminResponse>>>
{
    public Task<Result<IReadOnlyList<CouponAdminResponse>>> Handle(
        GetActiveCouponsQuery request, CancellationToken cancellationToken)
    {
        CouponAdminSeeds.Ensure();
        var count = Math.Clamp(request.Count <= 0 ? 8 : request.Count, 1, 50);
        var items = AdminCrudStore<CouponAdminResponse>.All()
            .Where(c => c.Status == "Active")
            .OrderByDescending(c => c.CreatedAt)
            .Take(count)
            .ToArray();

        return Task.FromResult(
            Result.Success<IReadOnlyList<CouponAdminResponse>>(items));
    }
}

public sealed class GetActiveBannersQueryHandler
    : IQueryHandler<GetActiveBannersQuery, Result<IReadOnlyList<BannerResponse>>>
{
    private readonly IAdminRepository<Banner> _banners;

    public GetActiveBannersQueryHandler(IAdminRepository<Banner> banners) => _banners = banners;

    public async Task<Result<IReadOnlyList<BannerResponse>>> Handle(
        GetActiveBannersQuery request, CancellationToken cancellationToken)
    {
        var count = Math.Clamp(request.Count <= 0 ? 8 : request.Count, 1, 50);
        var items = await _banners.ListAsync(
            b => b.Status == "Active",
            q => q.OrderByDescending(b => b.CreatedAt),
            cancellationToken);

        return Result.Success<IReadOnlyList<BannerResponse>>(
            items.Select(b => b.ToDto()).Take(count).ToArray());
    }
}

public sealed class GetActiveCartRulesQueryHandler
    : IQueryHandler<GetActiveCartRulesQuery, Result<IReadOnlyList<CartRuleResponse>>>
{
    private readonly IAdminRepository<CartRule> _rules;

    public GetActiveCartRulesQueryHandler(IAdminRepository<CartRule> rules) => _rules = rules;

    public async Task<Result<IReadOnlyList<CartRuleResponse>>> Handle(
        GetActiveCartRulesQuery request, CancellationToken cancellationToken)
    {
        var count = Math.Clamp(request.Count <= 0 ? 8 : request.Count, 1, 50);
        var items = await _rules.ListAsync(
            r => r.Status == "Active",
            q => q.OrderBy(r => r.Priority).ThenByDescending(r => r.CreatedAt),
            cancellationToken);

        return Result.Success<IReadOnlyList<CartRuleResponse>>(
            items.Select(r => r.ToDto()).Take(count).ToArray());
    }
}

public sealed class GetActiveBulkTiersQueryHandler
    : IQueryHandler<GetActiveBulkTiersQuery, Result<IReadOnlyList<WholesalePriceResponse>>>
{
    public Task<Result<IReadOnlyList<WholesalePriceResponse>>> Handle(
        GetActiveBulkTiersQuery request, CancellationToken cancellationToken)
    {
        // Same seed-backed shelf the admin console manages; served publicly
        // so guests see the bulk-order savings without signing in.
        WholesalePriceSeeds.Ensure();
        var count = Math.Clamp(request.Count <= 0 ? 8 : request.Count, 1, 50);
        var items = AdminCrudStore<WholesalePriceResponse>.All()
            .OrderBy(p => p.ProductName)
            .Take(count)
            .ToArray();

        return Task.FromResult(Result.Success<IReadOnlyList<WholesalePriceResponse>>(items));
    }
}
