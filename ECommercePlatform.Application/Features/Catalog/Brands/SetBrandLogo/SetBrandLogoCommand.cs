using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Catalog.Brands.SetBrandLogo;

/// <summary>
/// Replaces a brand's primary logo from a single uploaded file, without
/// requiring the full multipart update payload.
/// </summary>
public sealed record SetBrandLogoCommand : ICommand<Result<BrandResponse>>
{
    /// <summary>Comes from the route, not the body.</summary>
    [JsonIgnore]
    public Guid BrandId { get; init; }

    public FileUpload? File { get; init; }
}
