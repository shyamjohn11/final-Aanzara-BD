using System.Security.Claims;
using ECommercePlatform.Application.Common.Security;
using ECommercePlatform.Infrastructure.Security;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ECommercePlatform.Api.Common;

/// <summary>
/// Reads the authenticated caller off the current HttpContext. The Application
/// layer depends on <see cref="ICurrentUser"/>, never on this.
/// </summary>
public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId => ParseGuid(
        FindFirst(JwtRegisteredClaimNames.Sub) ?? FindFirst(ClaimTypes.NameIdentifier));

    public Guid? SessionId => ParseGuid(FindFirst(AppClaimTypes.SessionId));

    public string? Email => FindFirst(JwtRegisteredClaimNames.Email) ?? FindFirst(ClaimTypes.Email);

    private string? FindFirst(string claimType) => Principal?.FindFirst(claimType)?.Value;

    private static Guid? ParseGuid(string? value)
        => Guid.TryParse(value, out var parsed) ? parsed : null;
}
