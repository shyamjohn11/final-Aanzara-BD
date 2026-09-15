using ECommercePlatform.Api.Common;
using ECommercePlatform.Api.Extensions;
using ECommercePlatform.Api.Security;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Application.Features.Catalog.SubCategories.CreateSubCategory;
using ECommercePlatform.Application.Features.Catalog.SubCategories.DeleteSubCategory;
using ECommercePlatform.Application.Features.Catalog.SubCategories.GetSubCategories;
using ECommercePlatform.Application.Features.Catalog.SubCategories.UpdateSubCategory;
using ECommercePlatform.Application.Features.Catalog.SubCategoryImages.GetSubCategoryImageFile;
using ECommercePlatform.Application.Features.Catalog.SubCategoryImages.SetSubCategoryImage;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

/// <summary>Middle level of the product tree.</summary>
[Authorize]
[Route("api/v1/subcategories")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
public sealed class SubCategoriesController : ApiControllerBase
{
    private readonly ILogger<SubCategoriesController> _logger;

    public SubCategoriesController(ISender sender, ILogger<SubCategoriesController> logger)
        : base(sender)
        => _logger = logger;

    /// <summary>Paged list, optionally scoped to one category.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<SubCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<SubCategoryResponse>>> List(
      [FromQuery] GetSubCategoriesQuery query,
      CancellationToken cancellationToken)
    {
        _logger.LogInformation("List action started.");

        try
        {
            var result = await Sender.Send(query, cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("List action finished.");
        }
    }

    /// <summary>Streams the sub-category's image file so guests can browse.</summary>
    [HttpGet("{subCategoryId:guid}/image/file")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetImageFile(
        Guid subCategoryId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetSubCategoryImageFileQuery(subCategoryId), cancellationToken);

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
        return PhysicalFile(file.FilePath, file.ContentType, file.FileName, enableRangeProcessing: true);
    }

    /// <summary>Creates a sub-category under an existing category.</summary>
    [HttpPost]
    [HasPermission(Permissions.Category.Create)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(SubCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SubCategoryResponse>> Create(
     [FromForm] CreateSubCategoryCommand command,
     IFormFile? image,
     CancellationToken cancellationToken)
    {
        _logger.LogInformation("Create action started.");

        try
        {
            var result = await Sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return ToProblem(result.Error!);
            }

            // The image field used to be accepted here but never persisted.
            // Save it now that the sub-category row exists.
            if (image is not null)
            {
                var imageResult = await Sender.Send(
                    new SetSubCategoryImageCommand
                    {
                        SubCategoryId = result.Value.SubCategoryId,
                        File = ToFileUpload(image)
                    },
                    cancellationToken);

                if (imageResult.IsFailure)
                {
                    return ToProblem(imageResult.Error!);
                }

                return Ok(imageResult.Value);
            }

            return Ok(result.Value);
        }
        finally
        {
            _logger.LogInformation("Create action finished.");
        }
    }

    /// <summary>Updates a sub-category in place.</summary>
    [HttpPut("{subCategoryId:guid}")]
    [HasPermission(Permissions.Category.Update)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(SubCategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubCategoryResponse>> Update(
     Guid subCategoryId,
     [FromForm] UpdateSubCategoryCommand command,
     IFormFile? image,
     CancellationToken cancellationToken)
    {
        _logger.LogInformation("Update action started.");

        try
        {
            var result = await Sender.Send(
                command with { SubCategoryId = subCategoryId }, cancellationToken);

            if (result.IsFailure)
            {
                return ToProblem(result.Error!);
            }

            // Same fix as create: a sent image replaces the primary one.
            if (image is not null)
            {
                var imageResult = await Sender.Send(
                    new SetSubCategoryImageCommand
                    {
                        SubCategoryId = subCategoryId,
                        File = ToFileUpload(image)
                    },
                    cancellationToken);

                if (imageResult.IsFailure)
                {
                    return ToProblem(imageResult.Error!);
                }

                return Ok(imageResult.Value);
            }

            return Ok(result.Value);
        }
        finally
        {
            _logger.LogInformation("Update action finished.");
        }
    }

    /// <summary>Deletes a sub-category. No payload. Refused while it still has products.</summary>    [HttpDelete("{subCategoryId:guid}")]
    [HasPermission(Permissions.Category.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(
     Guid subCategoryId,
     CancellationToken cancellationToken)
    {
        _logger.LogInformation("Delete action started.");

        try
        {
            var result = await Sender.Send(
                new DeleteSubCategoryCommand(subCategoryId), cancellationToken);

            return ToNoContent(result);
        }
        finally
        {
            _logger.LogInformation("Delete action finished.");
        }
    }

    /// <summary>Maps the multipart file to the storage-agnostic upload shape.</summary>
    private static FileUpload ToFileUpload(IFormFile file) => new(
        file.OpenReadStream(),
        file.FileName,
        string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
        file.Length);
}