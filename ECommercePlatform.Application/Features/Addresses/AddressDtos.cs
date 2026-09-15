using ECommercePlatform.Domain.Entities;

using ECommercePlatform.Domain.Errors;
namespace ECommercePlatform.Application.Features.Addresses;

public sealed record AddressResponse
{
    public Guid AddressId { get; init; }
    public string? Label { get; init; }
    public string AddressLine1 { get; init; } = string.Empty;
    public string? AddressLine2 { get; init; }
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string Pincode { get; init; } = string.Empty;
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public bool IsDefault { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    public static AddressResponse From(Address address) => new()
    {
        AddressId = address.AddressId,
        Label = address.Label,
        AddressLine1 = address.AddressLine1,
        AddressLine2 = address.AddressLine2,
        City = address.City,
        State = address.State,
        Pincode = address.Pincode,
        Latitude = address.Latitude,
        Longitude = address.Longitude,
        IsDefault = address.IsDefault,
        CreatedAt = address.CreatedAt
    };
}
