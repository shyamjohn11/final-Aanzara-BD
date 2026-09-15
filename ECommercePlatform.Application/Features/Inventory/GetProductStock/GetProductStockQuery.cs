using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Inventory.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Inventory.GetProductStock;

public sealed record GetProductStockQuery(Guid ProductId, Guid? WarehouseId = null)
    : IQuery<Result<ProductStockDetailResponse>>;
