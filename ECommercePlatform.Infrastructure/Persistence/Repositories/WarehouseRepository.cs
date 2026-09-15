using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

public sealed class WarehouseRepository : IWarehouseRepository
{
    private readonly ApplicationDbContext _db;

    public WarehouseRepository(ApplicationDbContext db) => _db = db;

    public Task<Warehouse?> GetByIdAsync(Guid warehouseId, CancellationToken cancellationToken)
        => _db.Warehouses.FirstOrDefaultAsync(w => w.WarehouseId == warehouseId, cancellationToken);

    public Task<bool> ExistsAsync(Guid warehouseId, CancellationToken cancellationToken)
        => _db.Warehouses.AnyAsync(w => w.WarehouseId == warehouseId, cancellationToken);

    public Task<bool> NameExistsAsync(string warehouseName, Guid? excludingId, CancellationToken cancellationToken)
        => _db.Warehouses.AnyAsync(
            w => w.WarehouseName == warehouseName && (excludingId == null || w.WarehouseId != excludingId),
            cancellationToken);

    public Task<bool> HasInventoryAsync(Guid warehouseId, CancellationToken cancellationToken)
        => _db.Inventory.AnyAsync(i => i.WarehouseId == warehouseId, cancellationToken);

    public async Task<PagedResult<Warehouse>> SearchAsync(
        string? search, WarehouseStatus? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _db.Warehouses.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(w => EF.Functions.Like(w.WarehouseName, $"%{search}%"));
        }

        if (status is not null)
        {
            query = query.Where(w => w.Status == status);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(w => w.WarehouseName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Warehouse>(items, page, pageSize, total);
    }

    public void Add(Warehouse warehouse) => _db.Warehouses.Add(warehouse);

    public void Remove(Warehouse warehouse) => _db.Warehouses.Remove(warehouse);
}
