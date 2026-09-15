using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Common.Security;
using ECommercePlatform.Application.Features.Auth.Dtos;
using ECommercePlatform.Application.Features.Auth.Sessions;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Auth.Queries.GetProfile;

public sealed class GetProfileQueryHandler
    : IQueryHandler<GetProfileQuery, Result<UserProfileResponse>>
{
    private readonly IUserRepository _users;
    private readonly ISessionManager _sessionManager;
    private readonly IRoleService _roleService;

    public GetProfileQueryHandler(
        IUserRepository users, ISessionManager sessionManager, IRoleService roleService)
    {
        _users = users;
        _sessionManager = sessionManager;
        _roleService = roleService;
    }

    public async Task<Result<UserProfileResponse>> Handle(
        GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserProfileResponse>(AuthErrors.UserNotFound);
        }

        var roles = await _roleService.GetRolesForUserAsync(user.UserId, cancellationToken);

        return Result.Success(_sessionManager.MapProfile(user, roles));
    }
}