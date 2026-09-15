using ECommercePlatform.Api.Common;
using ECommercePlatform.Api.Extensions;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Banners;
using ECommercePlatform.Application.Features.Admin.Banners.GetBannerImageFile;
using ECommercePlatform.Application.Features.Admin.CartRules;
using ECommercePlatform.Application.Features.Admin.Combos;
using ECommercePlatform.Application.Features.Admin.Coupons;
using ECommercePlatform.Application.Features.Admin.Offers;
using ECommercePlatform.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

// Standard CRUD pages, same 5-endpoint shape as brands (#43-47).
// All Admin-role. Additive only: no existing controller is modified.

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/banners")]
public sealed class AdminBannersController : ApiControllerBase
{
    public AdminBannersController(ISender sender) : base(sender) { }

    [HttpGet] // #75 ?Page=&PageSize=
    [ProducesResponseType(typeof(PagedResult<BannerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<BannerResponse>>> List(
        [FromQuery] GetBannersQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query, ct));

    [HttpGet("{id:guid}", Name = "GetAdminBanner")] // #76
    [ProducesResponseType(typeof(BannerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BannerResponse>> Get(Guid id, CancellationToken ct)
        => ToResponse(await Sender.Send(new GetBannerByIdQuery(id), ct));

    [HttpPost] // #77 multipart: new banner with a picked file
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(BannerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BannerResponse>> CreateFromForm(
        [FromForm] CreateBannerForm form, CancellationToken ct)
    {
        var result = await Sender.Send(new CreateBannerCommand
        {
            Title = form.Title,
            Subtitle = form.Subtitle,
            Link = form.Link,
            Position = form.Position,
            Status = form.Status,
            StartDate = form.StartDate,
            EndDate = form.EndDate,
            ImageFile = ToFileUpload(form.Image),
            Extra = null
        }, ct);

        return result.IsFailure
            ? ToProblem(result.Error!)
            : CreatedAtRoute("GetAdminBanner", new { id = result.Value.Id }, result.Value);
    }

    [HttpPost] // #77 JSON: same fields, image arrives as URL string
    [Consumes("application/json")]
    [ProducesResponseType(typeof(BannerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BannerResponse>> CreateFromJson(
        [FromBody] CreateBannerCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return result.IsFailure
            ? ToProblem(result.Error!)
            : CreatedAtRoute("GetAdminBanner", new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")] // #78 multipart: edit with a new file (else old image kept)
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(BannerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BannerResponse>> UpdateFromForm(
        Guid id, [FromForm] UpdateBannerForm form, CancellationToken ct)
        => ToResponse(await Sender.Send(new UpdateBannerCommand
        {
            Id = id,
            Title = form.Title,
            Subtitle = form.Subtitle,
            Link = form.Link,
            Position = form.Position,
            Status = form.Status,
            StartDate = form.StartDate,
            EndDate = form.EndDate,
            ImageFile = ToFileUpload(form.Image),
            Extra = null
        }, ct));

    [HttpPut("{id:guid}")] // #78 JSON: edit without a new file (image URL kept)
    [Consumes("application/json")]
    [ProducesResponseType(typeof(BannerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BannerResponse>> UpdateFromJson(
        Guid id, [FromBody] UpdateBannerCommand command, CancellationToken ct)
        => ToResponse(await Sender.Send(command with { Id = id }, ct));

    [HttpPatch("{id:guid}/status")] // status toggle {status}
    [ProducesResponseType(typeof(BannerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BannerResponse>> UpdateStatus(
        Guid id, [FromBody] BannerStatusRequest request, CancellationToken ct)
        => ToResponse(await Sender.Send(
            new UpdateBannerStatusCommand(id, request.Status ?? string.Empty), ct));

    [HttpDelete("{id:guid}")] // #79 row + image file — 204
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => ToNoContent(await Sender.Send(new DeleteBannerCommand(id), ct));

    /// <summary>Streams the banner's stored image file, resolved by banner id alone.</summary>
    [HttpGet("{id:guid}/image/file")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetImageFile(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new GetBannerImageFileQuery(id), ct);

        if (result.IsFailure)
        {
            return ToProblem(result.Error!);
        }

        var file = result.Value;

        if (!System.IO.File.Exists(file.FilePath))
        {
            return ToProblem(ECommercePlatform.Domain.Errors.Error.NotFound(
                "admin.banner_image_file_missing",
                "The image record exists but its file is no longer available."));
        }

        Response.SetImageRevalidationCacheHeaders();

        // enableRangeProcessing lets browsers seek/partially fetch large images.
        return PhysicalFile(file.FilePath, file.ContentType, file.FileName, enableRangeProcessing: true);
    }

    private static FileUpload? ToFileUpload(IFormFile? file) => file is null
        ? null
        : new FileUpload(
            file.OpenReadStream(),
            file.FileName,
            string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/octet-stream"
                : file.ContentType,
            file.Length);
}

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/offers")]
public sealed class AdminOffersController : ApiControllerBase
{
    public AdminOffersController(ISender sender) : base(sender) { }

    [HttpGet] // #80
    [ProducesResponseType(typeof(PagedResult<OfferResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<OfferResponse>>> List(
        [FromQuery] GetOffersQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query, ct));

    [HttpGet("{id:guid}", Name = "GetAdminOffer")] // #81
    [ProducesResponseType(typeof(OfferResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<OfferResponse>> Get(Guid id, CancellationToken ct)
        => ToResponse(await Sender.Send(new GetOfferByIdQuery(id), ct));

    [HttpPost] // #82
    [ProducesResponseType(typeof(OfferResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<OfferResponse>> Create(
        [FromBody] CreateOfferCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return result.IsFailure
            ? ToProblem(result.Error!)
            : CreatedAtRoute("GetAdminOffer", new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")] // #83
    [ProducesResponseType(typeof(OfferResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<OfferResponse>> Update(
        Guid id, [FromBody] UpdateOfferCommand command, CancellationToken ct)
        => ToResponse(await Sender.Send(command with { Id = id }, ct));

    [HttpDelete("{id:guid}")] // #84
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => ToNoContent(await Sender.Send(new DeleteOfferCommand(id), ct));
}

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/coupons")]
public sealed class AdminCouponsController : ApiControllerBase
{
    public AdminCouponsController(ISender sender) : base(sender) { }

    [HttpGet] // #85
    [ProducesResponseType(typeof(PagedResult<CouponAdminResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CouponAdminResponse>>> List(
        [FromQuery] GetCouponsQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query, ct));

    [HttpGet("{id:guid}", Name = "GetAdminCoupon")] // #86
    [ProducesResponseType(typeof(CouponAdminResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CouponAdminResponse>> Get(Guid id, CancellationToken ct)
        => ToResponse(await Sender.Send(new GetCouponByIdQuery(id), ct));

    [HttpPost] // #87
    [ProducesResponseType(typeof(CouponAdminResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CouponAdminResponse>> Create(
        [FromBody] CreateCouponCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return result.IsFailure
            ? ToProblem(result.Error!)
            : CreatedAtRoute("GetAdminCoupon", new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")] // #88
    [ProducesResponseType(typeof(CouponAdminResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CouponAdminResponse>> Update(
        Guid id, [FromBody] UpdateCouponCommand command, CancellationToken ct)
        => ToResponse(await Sender.Send(command with { Id = id }, ct));

    [HttpDelete("{id:guid}")] // #89
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => ToNoContent(await Sender.Send(new DeleteCouponCommand(id), ct));
}

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/combos")]
public sealed class AdminCombosController : ApiControllerBase
{
    public AdminCombosController(ISender sender) : base(sender) { }

    [HttpGet] // #90
    [ProducesResponseType(typeof(PagedResult<ComboResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ComboResponse>>> List(
        [FromQuery] GetCombosQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query, ct));

    [HttpGet("{id:guid}", Name = "GetAdminCombo")] // #91
    [ProducesResponseType(typeof(ComboResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ComboResponse>> Get(Guid id, CancellationToken ct)
        => ToResponse(await Sender.Send(new GetComboByIdQuery(id), ct));

    [HttpPost] // #92 multipart: new combo with a picked file
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ComboResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ComboResponse>> CreateFromForm(
        [FromForm] CreateComboForm form, CancellationToken ct)
    {
        var result = await Sender.Send(new CreateComboCommand
        {
            Name = form.Name,
            Title = form.Title,
            Description = form.Description,
            ProductIds = form.ProductIds,
            Price = form.Price,
            OriginalPrice = form.OriginalPrice,
            Status = form.Status,
            ImageFile = AdminFormFiles.ToFileUpload(form.Image),
            Extra = null
        }, ct);

        return result.IsFailure
            ? ToProblem(result.Error!)
            : CreatedAtRoute("GetAdminCombo", new { id = result.Value.Id }, result.Value);
    }

    [HttpPost] // #92 JSON: same fields, image arrives as URL string
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ComboResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ComboResponse>> CreateFromJson(
        [FromBody] CreateComboCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return result.IsFailure
            ? ToProblem(result.Error!)
            : CreatedAtRoute("GetAdminCombo", new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")] // #93 multipart: edit with a new file (else old image kept)
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ComboResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ComboResponse>> UpdateFromForm(
        Guid id, [FromForm] UpdateComboForm form, CancellationToken ct)
        => ToResponse(await Sender.Send(new UpdateComboCommand
        {
            Id = id,
            Name = form.Name,
            Title = form.Title,
            Description = form.Description,
            ProductIds = form.ProductIds,
            Price = form.Price,
            OriginalPrice = form.OriginalPrice,
            Status = form.Status,
            ImageFile = AdminFormFiles.ToFileUpload(form.Image),
            Extra = null
        }, ct));

    [HttpPut("{id:guid}")] // #93 JSON: edit without a new file (image URL kept)
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ComboResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ComboResponse>> UpdateFromJson(
        Guid id, [FromBody] UpdateComboCommand command, CancellationToken ct)
        => ToResponse(await Sender.Send(command with { Id = id }, ct));

    [HttpDelete("{id:guid}")] // #94
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => ToNoContent(await Sender.Send(new DeleteComboCommand(id), ct));

    public sealed record UpdateStatusRequest(string Status);

    [HttpPatch("{id:guid}/status")] // activate/deactivate
    [ProducesResponseType(typeof(ComboResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ComboResponse>> UpdateStatus(
        Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
        => ToResponse(await Sender.Send(new UpdateComboStatusCommand(id, request.Status), ct));
}

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/cart-rules")]
public sealed class AdminCartRulesController : ApiControllerBase
{
    public AdminCartRulesController(ISender sender) : base(sender) { }

    [HttpGet] // #95
    [ProducesResponseType(typeof(PagedResult<CartRuleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CartRuleResponse>>> List(
        [FromQuery] GetCartRulesQuery query, CancellationToken ct)
        => ToResponse(await Sender.Send(query, ct));

    [HttpGet("{id:guid}", Name = "GetAdminCartRule")] // #96
    [ProducesResponseType(typeof(CartRuleResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartRuleResponse>> Get(Guid id, CancellationToken ct)
        => ToResponse(await Sender.Send(new GetCartRuleByIdQuery(id), ct));

    [HttpPost] // #97
    [ProducesResponseType(typeof(CartRuleResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CartRuleResponse>> Create(
        [FromBody] CreateCartRuleCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return result.IsFailure
            ? ToProblem(result.Error!)
            : CreatedAtRoute("GetAdminCartRule", new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")] // #98
    [ProducesResponseType(typeof(CartRuleResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartRuleResponse>> Update(
        Guid id, [FromBody] UpdateCartRuleCommand command, CancellationToken ct)
        => ToResponse(await Sender.Send(command with { Id = id }, ct));

    [HttpDelete("{id:guid}")] // #99
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        => ToNoContent(await Sender.Send(new DeleteCartRuleCommand(id), ct));

    public sealed record UpdateStatusRequest(string Status);

    [HttpPatch("{id:guid}/status")] // activate/deactivate
    [ProducesResponseType(typeof(CartRuleResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartRuleResponse>> UpdateStatus(
        Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
        => ToResponse(await Sender.Send(new UpdateCartRuleStatusCommand(id, request.Status), ct));
}
