using ECommercePlatform.Application.Common.Messaging;

using ECommercePlatform.Domain.Errors;
namespace ECommercePlatform.Application.Features.Addresses.CreateAddress;

public sealed record CreateAddressCommand(
    Guid UserId,
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
