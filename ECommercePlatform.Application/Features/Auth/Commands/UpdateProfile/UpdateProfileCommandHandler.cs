using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Common.Security;
using ECommercePlatform.Application.Features.Auth.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Auth.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandler
    : ICommandHandler<UpdateProfileCommand, Result<UserProfileResponse>>
{
    private const int MinNameLength = 2;

    private readonly IUserRepository _users;
    private readonly IRoleService _roleService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProfileCommandHandler(
        IUserRepository users,
        IRoleService roleService,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _roleService = roleService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserProfileResponse>> Handle(
        UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserProfileResponse>(AuthErrors.UserNotFound);
        }

        if (request.Name.Trim().Length < MinNameLength)
        {
            return Result.Failure<UserProfileResponse>(Error.Validation(
                "auth.profile_name_too_short",
                $"Name must be at least {MinNameLength} characters."));
        }

        if (request.DateOfBirth is { } dob && dob > DateTimeOffset.UtcNow)
        {
            return Result.Failure<UserProfileResponse>(Error.Validation(
                "auth.profile_dob_future",
                "Date of birth cannot be in the future."));
        }

        user.Name = request.Name.Trim();
        user.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        user.DateOfBirth = request.DateOfBirth;
        user.Gender = string.IsNullOrWhiteSpace(request.Gender) ? null : request.Gender.Trim();

        await _users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var roles = await _roleService.GetRolesForUserAsync(user.UserId, cancellationToken);

        return Result.Success(new UserProfileResponse
        {
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            AvatarUrl = user.AvatarUrl,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            Status = user.Status,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Roles = roles
        });
    }
}
