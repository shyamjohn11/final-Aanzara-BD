namespace ECommercePlatform.Application.Features.Users.Dtos;

public sealed record UserDetailsResponse(
    Guid UserId,
    string Name,
    string Email,
    string Role,
    string Status);