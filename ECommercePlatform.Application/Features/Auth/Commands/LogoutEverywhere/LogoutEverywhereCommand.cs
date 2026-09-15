using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Auth.Commands.LogoutEverywhere;

/// <summary>Ends every active session for the caller. Returns the number revoked.</summary>
public sealed record LogoutEverywhereCommand : AuthenticatedCommand, ICommand<Result<int>>;
