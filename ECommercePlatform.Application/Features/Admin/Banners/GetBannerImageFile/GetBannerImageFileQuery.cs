using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.Banners.GetBannerImageFile;

/// <summary>Resolves a banner's stored image file for streaming.</summary>
public sealed record GetBannerImageFileQuery(Guid Id) : IQuery<Result<BannerImageFileResponse>>;

/// <summary>Physical file behind a banner imageUrl. Never serialized as JSON —
/// this backs a raw file response, mirroring the category image-file flow.</summary>
public sealed record BannerImageFileResponse(string FilePath, string ContentType, string FileName);
