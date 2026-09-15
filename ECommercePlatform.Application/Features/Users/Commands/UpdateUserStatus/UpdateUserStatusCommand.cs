using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Users.Dtos;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Users.Commands.UpdateUserStatus;

public sealed record UpdateUserStatusCommand(Guid UserId, string Status) : ICommand<Result<UpdateUserStatusResponse>>;