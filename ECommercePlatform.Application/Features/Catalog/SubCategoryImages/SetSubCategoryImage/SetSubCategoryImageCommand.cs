using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.SubCategoryImages.SetSubCategoryImage;

/// <summary>
/// Persists the uploaded file sent alongside sub-category create/update.
/// The multipart <c>image</c> field on those actions previously reached the
/// controller but was never mapped into a command, so the file was silently
/// dropped while SubCategoryImages stayed empty.
/// </summary>
public sealed record SetSubCategoryImageCommand : ICommand<Result<SubCategoryResponse>>
{
    /// <summary>Comes from the route, not the body.</summary>
    [JsonIgnore]
    public Guid SubCategoryId { get; init; }

    public FileUpload? File { get; init; }
}
