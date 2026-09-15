using ECommercePlatform.Api.Common;
using ECommercePlatform.Api.Extensions;
using ECommercePlatform.Api.Security;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog;
using ECommercePlatform.Application.Features.Catalog.CategoryImages.DeleteCategoryImage;
using ECommercePlatform.Application.Features.Catalog.CategoryImages.GetCategoryImageFileById;
using ECommercePlatform.Application.Features.Catalog.CategoryImages.GetCategoryImages;
using ECommercePlatform.Application.Features.Catalog.CategoryImages.SetPrimaryImage;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

/// <summary>
/// Operations on an individual image. Listing and adding live under
/// /categories/{id}/images, since those are scoped to a category.
/// </summary>
[Authorize]
[Route("api/v1/category-images")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public sealed class CategoryImagesController : ApiControllerBase
{
    private readonly ILogger<CategoryImagesController> _logger;

    public CategoryImagesController(ISender sender, ILogger<CategoryImagesController> logger)
        : base(sender)
        => _logger = logger;

    /// <summary>Lists a category's images, in display order.</summary>
    [HttpGet]
    [HasPermission(Permissions.Category.View)]
    [ProducesResponseType(typeof(IReadOnlyCollection<CategoryImageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<CategoryImageResponse>>> List(
        Guid categoryId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("List images action started.");

        try
        {
            var result = await Sender.Send(new GetCategoryImagesQuery(categoryId), cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("List images action finished.");
        }
    }

    /// <summary>Promotes an image to primary, demoting the incumbent.</summary>
    [HttpPost("{categoryImageId:guid}/primary")]
    [ProducesResponseType(typeof(CategoryImageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryImageResponse>> SetPrimary(
        Guid categoryImageId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SetPrimary action started.");

        try
        {
            var result = await Sender.Send(
                new SetPrimaryImageCommand(categoryImageId), cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("SetPrimary action finished.");
        }
    }

    /// <summary>
    /// Deletes an image. No payload. Promotes the next image if this one was primary.
    /// </summary>
    [HttpDelete("{categoryImageId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid categoryImageId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Delete action started.");

        try
        {
            var result = await Sender.Send(
                new DeleteCategoryImageCommand(categoryImageId), cancellationToken);

            return ToNoContent(result);
        }
        finally
        {
            _logger.LogInformation("Delete action finished.");
        }
    }

    /// <summary>
    /// Streams one image's bytes. Anonymous so gallery images render on public
    /// pages — stored URLs are not directly servable.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{categoryImageId:guid}/file")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetImageFile(Guid categoryImageId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetImageFile action started.");

        try
        {
            var result = await Sender.Send(
                new GetCategoryImageFileByIdQuery(categoryImageId), cancellationToken);

            if (result.IsFailure)
            {
                return ToProblem(result.Error!);
            }

            var file = result.Value;

            if (!System.IO.File.Exists(file.FilePath))
            {
                return ToProblem(CatalogErrors.ImageFileMissing);
            }

            Response.SetImageRevalidationCacheHeaders();

            // enableRangeProcessing lets browsers seek/partially fetch large images.
            return PhysicalFile(file.FilePath, file.ContentType, file.FileName, enableRangeProcessing: true);
        }
        finally
        {
            _logger.LogInformation("GetImageFile action finished.");
        }
    }
}
