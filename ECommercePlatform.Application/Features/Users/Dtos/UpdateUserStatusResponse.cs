namespace ECommercePlatform.Application.Features.Users.Dtos;

public sealed record UpdateUserStatusResponse(
    Guid UserId,
    string Status);