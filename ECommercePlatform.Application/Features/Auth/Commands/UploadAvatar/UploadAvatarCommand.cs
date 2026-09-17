using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Auth.Commands.UploadAvatar;

public sealed record UploadAvatarCommand : AuthenticatedCommand, ICommand<Result<AvatarResponse>>
{
    /// <summary>Bound by the controller from the multipart form, never from JSON.</summary>
    [JsonIgnore]
    public FileUpload? File { get; init; }
}

public sealed record AvatarResponse(string AvatarUrl);
