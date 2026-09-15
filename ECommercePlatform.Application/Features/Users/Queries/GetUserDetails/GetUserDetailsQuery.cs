using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Users.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Users.Queries.GetUserDetails;

public sealed record GetUserDetailsQuery(Guid UserId) : IQuery<Result<UserDetailsResponse>>;