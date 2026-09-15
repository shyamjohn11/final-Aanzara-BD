using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Auth.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Auth.Queries.GetProfile;

/// <summary>The caller's own profile. The id comes from the token, never the request.</summary>
public sealed record GetProfileQuery(Guid UserId) : IQuery<Result<UserProfileResponse>>;
