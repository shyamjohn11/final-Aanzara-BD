namespace ECommercePlatform.Application.Features.Users.Dtos;

public sealed record UpdateUserRoleResponse(
    Guid UserId,
    string Role);