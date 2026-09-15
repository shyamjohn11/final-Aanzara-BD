using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

public sealed class InventoryRepository : IInventoryRepository
{
    private readonly ApplicationDbContext _db;

    public InventoryRepository(ApplicationDbContext db) => _db = db;

    public Task<Inventory?> GetByProductAndWarehouseAsync(
        Guid productId, Guid warehouseId, CancellationToken cancellationToken)
        => _db.Inventory
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .FirstOrDefaultAsync(i => i.ProductId == productId && i.WarehouseId == warehouseId, cancellationToken);

    public async Task<IReadOnlyCollection<Inventory>> GetByProductAsync(
        Guid productId, Guid? warehouseId, CancellationToken cancellationToken)
    {
        var query = _db.Inventory
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.ProductId == productId);

        if (warehouseId is not null)
        {
            query = query.Where(i => i.WarehouseId == warehouseId);
        }

        return await query.ToListAsync(cancellationToken);
    }

        public async Task<IReadOnlyCollection<Inventory>> GetByProductIdsAsync(
        IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
        {
            return [];
        }

        return await _db.Inventory
            .AsNoTracking()
            .Where(i => productIds.Contains(i.ProductId))
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<Inventory>> SearchInventoryAsync(
        Guid? warehouseId, Guid? productId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _db.Inventory
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .AsQueryable();

        if (warehouseId is not null)
        {
            query = query.Where(i => i.WarehouseId == warehouseId);
        }

        if (productId is not null)
        {
            query = query.Where(i => i.ProductId == productId);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(i => i.Product.ProductName)
            .ThenBy(i => i.Warehouse.WarehouseName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Inventory>(items, page, pageSize, total);
    }

    public async Task<PagedResult<Inventory>> SearchLowStockAsync(
        Guid? warehouseId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _db.Inventory
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.StockQuantity <= i.ReorderLevel);

        if (warehouseId is not null)
        {
            query = query.Where(i => i.WarehouseId == warehouseId);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(i => i.StockQuantity)
            .ThenBy(i => i.Product.ProductName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Inventory>(items, page, pageSize, total);
    }

    public async Task<PagedResult<InventoryMovement>> SearchMovementsAsync(
        Guid productId, Guid? warehouseId, DateTimeOffset? from, DateTimeOffset? to, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _db.InventoryMovements
            .AsNoTracking()
            .Include(m => m.Product)
            .Include(m => m.Warehouse)
            .Where(m => m.ProductId == productId);

        if (warehouseId is not null)
        {
            query = query.Where(m => m.WarehouseId == warehouseId);
        }

        if (from is not null)
        {
            query = query.Where(m => m.CreatedAt >= from);
        }

        if (to is not null)
        {
            query = query.Where(m => m.CreatedAt <= to);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<InventoryMovement>(items, page, pageSize, total);
    }

    public void Add(Inventory inventory) => _db.Inventory.Add(inventory);

    public void AddMovement(InventoryMovement movement) => _db.InventoryMovements.Add(movement);
}
