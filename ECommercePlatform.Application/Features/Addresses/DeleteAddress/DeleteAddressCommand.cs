using ECommercePlatform.Application.Common.Messaging;

using ECommercePlatform.Domain.Errors;
namespace ECommercePlatform.Application.Features.Addresses.DeleteAddress;

public sealed record DeleteAddressCommand(Guid UserId, Guid AddressId)
    : ICommand<Result>;
