using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog;
using ECommercePlatform.Application.Features.Inventory.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Inventory.GetInventoryMovements;

public sealed class GetInventoryMovementsQueryHandler
    : IQueryHandler<GetInventoryMovementsQuery, Result<PagedResult<InventoryMovementResponse>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IInventoryRepository _inventoryRepository;

    public GetInventoryMovementsQueryHandler(
        IProductRepository productRepository,
        IInventoryRepository inventoryRepository)
    {
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
    }

    public async Task<Result<PagedResult<InventoryMovementResponse>>> Handle(
        GetInventoryMovementsQuery request, CancellationToken cancellationToken)
    {
        if (await _productRepository.GetByIdAsync(request.ProductId, cancellationToken) is null)
        {
            return Result.Failure<PagedResult<InventoryMovementResponse>>(CatalogErrors.ProductNotFound);
        }

        var page = await _inventoryRepository.SearchMovementsAsync(
            request.ProductId,
            request.WarehouseId,
            request.From,
            request.To,
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result.Success(new PagedResult<InventoryMovementResponse>(
            page.Items.Select(m => m.ToResponse()).ToArray(),
            page.Page,
            page.PageSize,
            page.TotalCount));
    }
}
