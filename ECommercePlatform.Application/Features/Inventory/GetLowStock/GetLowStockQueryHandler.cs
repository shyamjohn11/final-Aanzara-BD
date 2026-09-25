using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Inventory.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Inventory.GetLowStock;

public sealed class GetLowStockQueryHandler
    : IQueryHandler<GetLowStockQuery, Result<PagedResult<InventoryResponse>>>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductImageRepository _productImages;

    public GetLowStockQueryHandler(
        IInventoryRepository inventoryRepository,
        IProductImageRepository productImages)
    {
        _inventoryRepository = inventoryRepository;
        _productImages = productImages;
    }

    public async Task<Result<PagedResult<InventoryResponse>>> Handle(
        GetLowStockQuery request, CancellationToken cancellationToken)
    {
        var page = await _inventoryRepository.SearchLowStockAsync(
            request.WarehouseId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = await page.Items.ToEnrichedResponsesAsync(_productImages, cancellationToken);

        return Result.Success(new PagedResult<InventoryResponse>(
            items,
            page.Page,
            page.PageSize,
            page.TotalCount));
    }
}
