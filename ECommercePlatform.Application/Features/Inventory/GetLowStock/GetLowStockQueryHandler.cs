using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Inventory.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Inventory.GetLowStock;

public sealed class GetLowStockQueryHandler
    : IQueryHandler<GetLowStockQuery, Result<PagedResult<InventoryResponse>>>
{
    private readonly IInventoryRepository _inventoryRepository;

    public GetLowStockQueryHandler(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<Result<PagedResult<InventoryResponse>>> Handle(
        GetLowStockQuery request, CancellationToken cancellationToken)
    {
        var page = await _inventoryRepository.SearchLowStockAsync(
            request.WarehouseId,
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result.Success(new PagedResult<InventoryResponse>(
            page.Items.Select(i => i.ToResponse()).ToArray(),
            page.Page,
            page.PageSize,
            page.TotalCount));
    }
}
