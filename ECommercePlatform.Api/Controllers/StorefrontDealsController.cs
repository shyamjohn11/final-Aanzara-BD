using ECommercePlatform.Api.Common;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Combos;
using ECommercePlatform.Application.Features.Admin.Banners;
using ECommercePlatform.Application.Features.Admin.CartRules;
using ECommercePlatform.Application.Features.Admin.WholesalePricing;
using ECommercePlatform.Application.Features.Admin.Coupons;
using ECommercePlatform.Application.Features.Admin.Offers;
using ECommercePlatform.Application.Features.Shop.Deals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

/// <summary>
/// Public storefront reads over the admin-managed marketing tables
/// (offers/combos/coupons). Active items only, newest first — no
/// authentication required, mirroring the product shelf endpoints.
/// </summary>
[AllowAnonymous]
[Route("api/v1/deals")]
public sealed class StorefrontDealsController : ApiControllerBase
{
    private readonly ILogger<StorefrontDealsController> _logger;

    public StorefrontDealsController(ISender sender, ILogger<StorefrontDealsController> logger)
        : base(sender)
        => _logger = logger;

    /// <summary>Active offers for the "Best Deals Right Now" shelf.</summary>
    [HttpGet("offers")]
    [ProducesResponseType(typeof(IReadOnlyList<OfferResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OfferResponse>>> GetActiveOffers(
        [FromQuery] int count = 8, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetActiveOffers action started.");

        try
        {
            var result = await Sender.Send(new GetActiveOffersQuery(count), cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("GetActiveOffers action finished.");
        }
    }

    /// <summary>Active combos for the storefront.</summary>
    [HttpGet("combos")]
    [ProducesResponseType(typeof(IReadOnlyList<ComboResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ComboResponse>>> GetActiveCombos(
        [FromQuery] int count = 8, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetActiveCombos action started.");

        try
        {
            var result = await Sender.Send(new GetActiveCombosQuery(count), cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("GetActiveCombos action finished.");
        }
    }

    /// <summary>Active coupon codes shoppers can apply.</summary>
    [HttpGet("coupons")]
    [ProducesResponseType(typeof(IReadOnlyList<CouponAdminResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CouponAdminResponse>>> GetActiveCoupons(
        [FromQuery] int count = 8, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetActiveCoupons action started.");

        try
        {
            var result = await Sender.Send(new GetActiveCouponsQuery(count), cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("GetActiveCoupons action finished.");
        }
    }

    /// <summary>Active banners for the storefront promo tiles.</summary>
    [HttpGet("banners")]
    [ProducesResponseType(typeof(IReadOnlyList<BannerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BannerResponse>>> GetActiveBanners(
        [FromQuery] int count = 8, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetActiveBanners action started.");

        try
        {
            var result = await Sender.Send(new GetActiveBannersQuery(count), cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("GetActiveBanners action finished.");
        }
    }

    /// <summary>Active cart rules for the bulk-pricing shelf.</summary>
    [HttpGet("cart-rules")]
    [ProducesResponseType(typeof(IReadOnlyList<CartRuleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CartRuleResponse>>> GetActiveCartRules(
        [FromQuery] int count = 8, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetActiveCartRules action started.");

        try
        {
            var result = await Sender.Send(new GetActiveCartRulesQuery(count), cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("GetActiveCartRules action finished.");
        }
    }

    /// <summary>Seed-backed wholesale price tiers for the bulk-order shelf.</summary>
    [HttpGet("bulk-tiers")]
    [ProducesResponseType(typeof(IReadOnlyList<WholesalePriceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WholesalePriceResponse>>> GetActiveBulkTiers(
        [FromQuery] int count = 8, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetActiveBulkTiers action started.");

        try
        {
            var result = await Sender.Send(new GetActiveBulkTiersQuery(count), cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("GetActiveBulkTiers action finished.");
        }
    }
}
