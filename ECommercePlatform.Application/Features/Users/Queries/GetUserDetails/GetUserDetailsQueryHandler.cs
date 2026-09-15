using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Users.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Users.Queries.GetUserDetails;

public sealed class GetUserDetailsQueryHandler
    : IQueryHandler<GetUserDetailsQuery, Result<UserDetailsResponse>>
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;

    public GetUserDetailsQueryHandler(IUserRepository users, IRoleRepository roles)
    {
        _users = users;
        _roles = roles;
    }

    public async Task<Result<UserDetailsResponse>> Handle(
        GetUserDetailsQuery request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserDetailsResponse>(UsersErrors.UserNotFound);
        }

        var roles = await _roles.GetRolesForUserAsync(user.UserId, cancellationToken);
        var role = roles.FirstOrDefault() ?? "Customer";

        var response = new UserDetailsResponse(
            user.UserId,
            user.Name,
            user.Email,
            role,
            user.Status);

        return Result.Success(response);
    }
}