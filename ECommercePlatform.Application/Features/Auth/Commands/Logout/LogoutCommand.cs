using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Auth.Commands.Logout;

/// <summary>Ends the session the calling access token belongs to. Carries no body.</summary>
public sealed record LogoutCommand : AuthenticatedCommand, ICommand<Result>;
