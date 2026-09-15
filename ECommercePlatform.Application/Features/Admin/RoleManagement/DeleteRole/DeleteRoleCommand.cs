using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Admin.RoleManagement.DeleteRole;

public sealed record DeleteRoleCommand(Guid RoleId) : ICommand<Result>;
