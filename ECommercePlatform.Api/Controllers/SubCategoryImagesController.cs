using ECommercePlatform.Api.Common;
using ECommercePlatform.Api.Security;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.SubCategoryImages.GetSubCategoryImages;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

/// <summary>Operations on sub-category images. Scoped under /api/v1/subcategory-images for discovery,
/// but images are managed independently per sub-category.</summary>
[Authorize]
[Route("api/v1/subcategory-images")]
public sealed class SubCategoryImagesController : ApiControllerBase
{
    private readonly ILogger<SubCategoryImagesController> _logger;

    public SubCategoryImagesController(ISender sender, ILogger<SubCategoryImagesController> logger)
        : base(sender)
        => _logger = logger;

    /// <summary>Lists a sub-category's images, in display order.</summary>
    [HttpGet("{subCategoryId:guid}")]
    [HasPermission(Permissions.Category.View)]
    [ProducesResponseType(typeof(IReadOnlyCollection<SubCategoryImageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<SubCategoryImageResponse>>> List(
        Guid subCategoryId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("List sub-category images action started.");

        try
        {
            var result = await Sender.Send(new GetSubCategoryImagesQuery(subCategoryId), cancellationToken);

            return ToResponse(result);
        }
        finally
        {
            _logger.LogInformation("List sub-category images action finished.");
        }
    }
}