namespace ECommercePlatform.Application.Common.Security;

/// <summary>
/// Ambient accessor for the authenticated caller. Handlers depend on this rather
/// than on anything HTTP-shaped, so they stay testable and transport-agnostic.
/// </summary>
/// <remarks>
/// There is no Roles member: the USERS table in the agreed schema carries no role
/// column, so authorization is currently authenticated-or-not. Adding roles means
/// adding storage for them first.
/// </remarks>
public interface ICurrentUser
{
    Guid? UserId { get; }

    Guid? SessionId { get; }

    string? Email { get; }

    bool IsAuthenticated { get; }
}
