using ECommercePlatform.Application.Common.Messaging;

using ECommercePlatform.Domain.Errors;
namespace ECommercePlatform.Application.Features.Addresses.UpdateAddress;

public sealed record UpdateAddressCommand(
    Guid UserId,
    Guid AddressId,
    string? Label,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string Pincode,
    decimal? Latitude,
    decimal? Longitude,
    bool IsDefault)
    : ICommand<Result<AddressResponse>>;
