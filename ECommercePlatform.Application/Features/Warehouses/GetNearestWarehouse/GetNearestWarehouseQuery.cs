using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Warehouses.GetNearestWarehouse;

public sealed record GetNearestWarehouseQuery(
    Guid ProductId,
    string City,
    string State,
    string? Pincode,
    double? Latitude,
    double? Longitude) : IQuery<Result<GetNearestWarehouseResponse>>;

public sealed record GetNearestWarehouseResponse(
    Guid WarehouseId,
    string WarehouseName,
    string City,
    string State,
    double? Latitude,
    double? Longitude,
    int AvailableStock,
    double DistanceKm);
