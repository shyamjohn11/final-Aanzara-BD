using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Auth.Commands.UploadAvatar;

public sealed class UploadAvatarCommandHandler
    : ICommandHandler<UploadAvatarCommand, Result<AvatarResponse>>
{
    private const long MaxAvatarBytes = 2 * 1024 * 1024;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] AllowedContentTypes =
        ["image/jpeg", "image/png", "image/webp"];

    private const string AvatarSubFolder = "avatars";

    private readonly IUserRepository _users;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UploadAvatarCommandHandler> _logger;

    public UploadAvatarCommandHandler(
        IUserRepository users,
        IFileStorageService fileStorage,
        IUnitOfWork unitOfWork,
        ILogger<UploadAvatarCommandHandler> logger)
    {
        _users = users;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<AvatarResponse>> Handle(
        UploadAvatarCommand request, CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length <= 0)
        {
            return Result.Failure<AvatarResponse>(Error.Validation(
                "auth.avatar_required", "An image file is required."));
        }

        if (request.File.Length > MaxAvatarBytes)
        {
            return Result.Failure<AvatarResponse>(Error.Validation(
                "auth.avatar_too_large", "Image size must be 2 MB or less."));
        }

        var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension)
            || !AllowedContentTypes.Contains(request.File.ContentType.ToLowerInvariant()))
        {
            return Result.Failure<AvatarResponse>(Error.Validation(
                "auth.avatar_type_not_allowed", "Please upload a JPG, JPEG, PNG or WEBP image."));
        }

        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<AvatarResponse>(AuthErrors.UserNotFound);
        }

        var stored = await _fileStorage.SaveAsync(request.File, AvatarSubFolder, cancellationToken);

        // Remove the previous avatar file so orphans don't accumulate.
        if (!string.IsNullOrWhiteSpace(user.AvatarUrl) && user.AvatarUrl != stored.Url)
        {
            await _fileStorage.DeleteByUrlAsync(user.AvatarUrl, cancellationToken);
        }

        user.AvatarUrl = stored.Url;
        await _users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} updated their avatar.", request.UserId);

        return Result.Success(new AvatarResponse(stored.Url));
    }
}
