using ECommercePlatform.Application.Common.Messaging;

using ECommercePlatform.Domain.Errors;
namespace ECommercePlatform.Application.Features.Addresses.GetMyAddresses;

public sealed record GetMyAddressesQuery(Guid UserId)
    : IQuery<Result<IReadOnlyList<AddressResponse>>>;
