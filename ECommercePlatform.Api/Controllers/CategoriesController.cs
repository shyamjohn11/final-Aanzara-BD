using ECommercePlatform.Api.Common;
using ECommercePlatform.Api.Extensions;
using ECommercePlatform.Api.Security;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.CategoryImages.AddCategoryImage;
using ECommercePlatform.Application.Features.Catalog.CategoryImages.GetCategoryImages;
using ECommercePlatform.Application.Features.Catalog.Categories.CreateCategory;
using ECommercePlatform.Application.Features.Catalog.Categories.DeleteCategory;
using ECommercePlatform.Application.Features.Catalog.Categories.GetCategories;
using ECommercePlatform.Application.Features.Catalog.Categories.GetCategoriesWithSubCategories;
using ECommercePlatform.Application.Features.Catalog.Categories.GetCategoryById;
using ECommercePlatform.Application.Features.Catalog.Categories.UpdateCategory;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommercePlatform.Application.Features.Catalog;
using ECommercePlatform.Application.Features.Catalog.CategoryImages.GetCategoryImageFile;
namespace ECommercePlatform.Api.Controllers;

/// <summary>Top level of the product tree.</summary>
[Authorize]
[Route("api/v1/categories")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
public sealed class CategoriesController : ApiControllerBase
{
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ISender sender, ILogger<CategoriesController> logger)
        : base(sender)
        => _logger = logger;

    /// <summary>Paged category list with optional search and active filter.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CategoryResponse>>> List(
        [FromQuery] GetCategoriesQuery query, CancellationToken cancellationToken)
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

    /// <summary>Lightweight lookup list of categories having sub-categories.</summary>
    [HttpGet("with-subcategories")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryLookupResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryLookupResponse>>> GetCategoriesWithSubCategories(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetCategoriesWithSubCategories action started.");

        try
        {
            var result = await Sender.Send(new GetCategoriesWithSubCategoriesQuery(), cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("GetCategoriesWithSubCategories action finished.");
        }
    }

    /// <summary>One category with its images.</summary>
    [HttpGet("{categoryId:guid}", Name = nameof(GetCategory))]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CategoryDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDetailResponse>> GetCategory(
        Guid categoryId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetCategory action started.");

        try
        {
            var result = await Sender.Send(new GetCategoryByIdQuery(categoryId), cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("GetCategory action finished.");
        }
    }

    /// <summary>Creates a category with its primary image in one request. Returns 201 with the created row.</summary>
    [HttpPost]
    [HasPermission(Permissions.Category.Create)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryResponse>> Create(
        [FromForm] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Create action started.");

        try
        {
            var command = new CreateCategoryCommand
            {
                CategoryCode = request.CategoryCode,
                CategoryName = request.CategoryName,
                Description = request.Description,
                HasSubCategory = request.HasSubCategory,
                IsActive = request.IsActive,
                Image = request.Image is null
                    ? null
                    : new FileUpload(
                        request.Image.OpenReadStream(),
                        request.Image.FileName,
                        request.Image.ContentType,
                        request.Image.Length)
            };

            var result = await Sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return ToProblem(result.Error!);
            }

            return CreatedAtAction(
                nameof(GetCategory),
                new { categoryId = result.Value.CategoryId },
                result.Value);
        }
        finally
        {
            _logger.LogInformation("Create action finished.");
        }
    }

    /// <summary>Updates a category in place, optionally replacing its primary image.</summary>
    [HttpPut("{categoryId:guid}")]
    [HasPermission(Permissions.Category.Update)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponse>> Update(
        Guid categoryId,
        [FromForm] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Update action started.");

        try
        {
            var command = new UpdateCategoryCommand
            {
                CategoryId = categoryId,
                CategoryCode = request.CategoryCode,
                CategoryName = request.CategoryName,
                Description = request.Description,
                IsActive = request.IsActive,
                Image = request.Image is null
                    ? null
                    : new FileUpload(
                        request.Image.OpenReadStream(),
                        request.Image.FileName,
                        request.Image.ContentType,
                        request.Image.Length),
                // HasSubCategory is not supported on the update command; keep existing value.
            };

            var result = await Sender.Send(command, cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("Update action finished.");
        }
    }

    /// <summary>
    /// Deletes a category. No payload. Refused while it still has sub-categories
    /// or products.
    /// </summary>
    [HttpDelete("{categoryId:guid}")]
    [HasPermission(Permissions.Category.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(Guid categoryId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Delete action started.");

        try
        {
            var result = await Sender.Send(new DeleteCategoryCommand(categoryId), cancellationToken);

            return ToNoContent(result);
        }
        finally
        {
            _logger.LogInformation("Delete action finished.");
        }
    }

    // -- Images, nested because an image has no meaning outside its category --

    /// <summary>Lists a category's images, in display order.</summary>
    [HttpGet("{categoryId:guid}/images")]
    [HasPermission(Permissions.Category.View)]
    [ProducesResponseType(typeof(IReadOnlyCollection<CategoryImageResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<CategoryImageResponse>>> ListImages(
        Guid categoryId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("ListImages action started.");

        try
        {
            var result = await Sender.Send(new GetCategoryImagesQuery(categoryId), cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("ListImages action finished.");
        }
    }

    /// <summary>Registers image metadata. The first image added becomes primary.</summary>
    [HttpPost("{categoryId:guid}/images")]
    [HasPermission(Permissions.Category.Update)]
    [ProducesResponseType(typeof(CategoryImageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryImageResponse>> AddImage(
        Guid categoryId,
        [FromBody] AddCategoryImageCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("AddImage action started.");

        try
        {
            var result = await Sender.Send(
                command with { CategoryId = categoryId }, cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("AddImage action finished.");
        }
    }
    /// <summary>Streams the primary image file for a category, resolved by category id alone.</summary>
    [HttpGet("{categoryId:guid}/image/file")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetImageFile(Guid categoryId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetImageFile action started.");

        try
        {
            var result = await Sender.Send(new GetCategoryImageFileQuery(categoryId), cancellationToken);

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