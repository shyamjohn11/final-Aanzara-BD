using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Users.Dtos;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Users.Commands.UpdateUserStatus;

public sealed class UpdateUserStatusCommandHandler
    : ICommandHandler<UpdateUserStatusCommand, Result<UpdateUserStatusResponse>>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserStatusCommandHandler(
        IUserRepository users,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateUserStatusResponse>> Handle(
        UpdateUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UpdateUserStatusResponse>(UsersErrors.UserNotFound);
        }

        if (request.Status != UserStatus.Active && request.Status != UserStatus.Inactive)
        {
            return Result.Failure<UpdateUserStatusResponse>(UsersErrors.InvalidStatus);
        }

        user.Status = request.Status;
        _users.Update(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateUserStatusResponse(user.UserId, user.Status));
    }
}