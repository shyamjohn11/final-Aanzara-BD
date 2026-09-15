using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Inventory.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Inventory.GetInventoryList;

public sealed class GetInventoryListQueryHandler
    : IQueryHandler<GetInventoryListQuery, Result<PagedResult<InventoryResponse>>>
{
    private readonly IInventoryRepository _inventoryRepository;

    public GetInventoryListQueryHandler(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<Result<PagedResult<InventoryResponse>>> Handle(
        GetInventoryListQuery request, CancellationToken cancellationToken)
    {
        var page = await _inventoryRepository.SearchInventoryAsync(
            request.WarehouseId,
            request.ProductId,
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
