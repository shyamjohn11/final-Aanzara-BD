using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Security;

namespace ECommercePlatform.Application.Features.Auth.Commands;

/// <summary>
/// Fields a command needs but a caller must never supply.
/// </summary>
/// <remarks>
/// Every property here is <see cref="JsonIgnoreAttribute"/>, which is load-bearing
/// security rather than tidiness: commands are model-bound straight from the
/// request body, so without it a caller could post <c>{"userId": "&lt;someone
/// else&gt;"}</c> and act as another user. The controller fills these in from the
/// validated token, using a <c>with</c> expression after binding.
/// </remarks>
public abstract record AuthenticatedCommand
{
    [JsonIgnore]
    public Guid UserId { get; init; }

    /// <summary>The session the calling access token belongs to.</summary>
    [JsonIgnore]
    public Guid? SessionId { get; init; }
}

/// <summary>
/// Same rule for the caller fingerprint on the anonymous credential commands:
/// recorded against the session for auditing, so it must come from the connection,
/// never from the body.
/// </summary>
public abstract record ClientAwareCommand
{
    [JsonIgnore]
    public ClientInfo Client { get; init; } = ClientInfo.Unknown;
}
