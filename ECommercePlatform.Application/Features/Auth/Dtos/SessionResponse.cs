namespace ECommercePlatform.Application.Features.Auth.Dtos;

public sealed record SessionResponse
{
    public Guid Id { get; init; }

    public DateTimeOffset CreatedAtUtc { get; init; }

    public DateTimeOffset ExpiresAtUtc { get; init; }

    public DateTimeOffset? LastUsedAtUtc { get; init; }

    public string? IpAddress { get; init; }

    public string? UserAgent { get; init; }

    /// <summary>True for the session the calling access token belongs to.</summary>
    public bool IsCurrent { get; init; }
}
