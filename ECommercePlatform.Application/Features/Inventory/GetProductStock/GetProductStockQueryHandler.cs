using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog;
using ECommercePlatform.Application.Features.Inventory.Dtos;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Inventory.GetProductStock;

public sealed class GetProductStockQueryHandler
    : IQueryHandler<GetProductStockQuery, Result<ProductStockDetailResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly IInventoryRepository _inventoryRepository;

    public GetProductStockQueryHandler(
        IProductRepository productRepository,
        IInventoryRepository inventoryRepository)
    {
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
    }

    public async Task<Result<ProductStockDetailResponse>> Handle(
        GetProductStockQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Failure<ProductStockDetailResponse>(CatalogErrors.ProductNotFound);
        }

        var inventories = await _inventoryRepository.GetByProductAsync(
            request.ProductId, request.WarehouseId, cancellationToken);

        var response = new ProductStockDetailResponse
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            Sku = product.Sku,
            TotalStockQuantity = inventories.Sum(i => i.StockQuantity),
            TotalReservedQuantity = inventories.Sum(i => i.ReservedQuantity),
            Warehouses = inventories.Select(i => i.ToWarehouseStockDto()).ToArray()
        };

        return Result.Success(response);
    }
}
