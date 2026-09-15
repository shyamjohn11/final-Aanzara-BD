using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Users;

public static class UsersErrors
{
    public static readonly Error UserNotFound =
        Error.NotFound("users.user_not_found", "The user could not be found.");

    public static readonly Error InvalidStatus =
        Error.Validation("users.invalid_status", "Status must be Active or Inactive.");

    public static readonly Error InvalidRole =
        Error.Validation("users.invalid_role", "Role must be Customer, Business, Agent, or Admin.");

    public static readonly Error RoleNotFound =
        Error.NotFound("users.role_not_found", "The specified role could not be found.");
}