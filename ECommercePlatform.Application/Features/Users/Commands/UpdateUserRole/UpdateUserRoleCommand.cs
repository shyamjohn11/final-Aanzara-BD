using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Users.Dtos;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Users.Commands.UpdateUserRole;

public sealed record UpdateUserRoleCommand(Guid UserId, string Role) : ICommand<Result<UpdateUserRoleResponse>>;