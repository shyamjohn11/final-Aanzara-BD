using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _db;

    public ProductRepository(ApplicationDbContext db) => _db = db;

    public Task<Product?> GetByIdAsync(Guid productId, CancellationToken cancellationToken)
        => _db.Products.FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);

    public Task<bool> SkuExistsAsync(
        string sku, Guid? excludingId, CancellationToken cancellationToken)
        => _db.Products.AnyAsync(
            p => p.Sku == sku && (excludingId == null || p.ProductId != excludingId),
            cancellationToken);

    public async Task<PagedResult<Product>> SearchAsync(
        ProductFilter filter, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _db.Products.AsNoTracking().AsQueryable();

        if (filter.SubCategoryId is not null)
        {
            query = query.Where(p => p.SubCategoryId == filter.SubCategoryId);
        }

        if (filter.CategoryId is not null)
        {
            query = query.Where(p => p.CategoryId == filter.CategoryId);
        }
        if (filter.BrandId is not null)
        {
            query = query.Where(p => p.BrandId == filter.BrandId);
        }
        if (filter.DealerId is not null)
        {
            query = query.Where(p => p.DealerId == filter.DealerId);
        }

        if (filter.Status is not null)
        {
            query = query.Where(p => p.Status == filter.Status);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(p =>
                EF.Functions.Like(p.ProductName, $"%{filter.Search}%")
                || EF.Functions.Like(p.Sku, $"%{filter.Search}%"));
        }

        if (filter.MinPrice is not null)
        {
            query = query.Where(p => p.Price >= filter.MinPrice);
        }

        if (filter.MaxPrice is not null)
        {
            query = query.Where(p => p.Price <= filter.MaxPrice);
        }

        var total = await query.CountAsync(cancellationToken);

        query = ApplySort(query, filter);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Product>(items, page, pageSize, total);
    }

    /// <summary>
    /// Maps the caller's sort key onto a column. An allow-list, not a dynamic
    /// expression: unrecognized input falls back to name rather than reaching the
    /// database.
    /// </summary>
    private static IQueryable<Product> ApplySort(IQueryable<Product> query, ProductFilter filter)
        => (filter.SortBy?.ToLowerInvariant(), filter.SortDescending) switch
        {
            ("sku", false) => query.OrderBy(p => p.Sku),
            ("sku", true) => query.OrderByDescending(p => p.Sku),
            ("mrp", false) => query.OrderBy(p => p.Mrp),
            ("mrp", true) => query.OrderByDescending(p => p.Mrp),
            ("price", false) => query.OrderBy(p => p.Price),
            ("price", true) => query.OrderByDescending(p => p.Price),
            ("created", false) => query.OrderBy(p => p.CreatedAt),
            ("created", true) => query.OrderByDescending(p => p.CreatedAt),
            ("discount", false) => query.OrderBy(p => p.Discount),
            ("discount", true) => query.OrderByDescending(p => p.Discount),
            (_, true) => query.OrderByDescending(p => p.ProductName),
            _ => query.OrderBy(p => p.ProductName)
        };

    public Task<int> CountByDealerAsync(Guid dealerId, CancellationToken cancellationToken)
        => _db.Products.CountAsync(p => p.DealerId == dealerId, cancellationToken);

    public void Add(Product product) => _db.Products.Add(product);

    public void Remove(Product product) => _db.Products.Remove(product);
}