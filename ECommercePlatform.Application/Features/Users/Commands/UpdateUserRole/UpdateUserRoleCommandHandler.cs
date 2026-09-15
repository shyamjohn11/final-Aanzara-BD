using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Users.Dtos;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Users.Commands.UpdateUserRole;

public sealed class UpdateUserRoleCommandHandler
    : ICommandHandler<UpdateUserRoleCommand, Result<UpdateUserRoleResponse>>
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserRoleCommandHandler(
        IUserRepository users,
        IRoleRepository roles,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _roles = roles;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateUserRoleResponse>> Handle(
        UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UpdateUserRoleResponse>(UsersErrors.UserNotFound);
        }

        var validRoles = new[] { Roles.Admin, Roles.Agent, Roles.Customer, Roles.BusinessAccount };
        if (!validRoles.Contains(request.Role))
        {
            return Result.Failure<UpdateUserRoleResponse>(UsersErrors.InvalidRole);
        }

        var allRoles = await _roles.GetAllAsync(cancellationToken);
        var role = allRoles.FirstOrDefault(r => r.RoleName == request.Role);

        if (role is null)
        {
            return Result.Failure<UpdateUserRoleResponse>(UsersErrors.RoleNotFound);
        }

        _roles.RemoveAdminUserRoles(user.UserId);
        _roles.AddAdminUserRole(user.UserId, role.RoleId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateUserRoleResponse(user.UserId, role.RoleName));
    }
}