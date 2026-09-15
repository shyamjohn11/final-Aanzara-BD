using System.Linq.Expressions;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic EF Core implementation for the admin CMS tables. Registered once as
/// <c>IAdminRepository&lt;&gt;</c>; one closed registration serves every entity.
/// </summary>
public sealed class AdminRepository<T> : IAdminRepository<T>
    where T : AdminEntity
{
    private readonly ApplicationDbContext _database;

    public AdminRepository(ApplicationDbContext database) => _database = database;

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _database.Set<T>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public void Add(T entity) => _database.Set<T>().Add(entity);

    public void Remove(T entity) => _database.Set<T>().Remove(entity);

    public Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        => _database.Set<T>().AsNoTracking().CountAsync(predicate, cancellationToken);

    public async Task<IReadOnlyList<T>> PageAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IOrderedQueryable<T>> order,
        int skip,
        int take,
        CancellationToken cancellationToken)
        => await order(_database.Set<T>().AsNoTracking().Where(predicate))
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IOrderedQueryable<T>> order,
        CancellationToken cancellationToken)
        => await order(_database.Set<T>().AsNoTracking().Where(predicate))
            .ToListAsync(cancellationToken);
}
