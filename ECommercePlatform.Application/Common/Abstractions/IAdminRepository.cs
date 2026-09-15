using System.Linq.Expressions;
using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Common.Abstractions;

/// <summary>
/// Narrow data-access surface for the admin CMS-style tables. Handlers compose
/// translatable filter/order expressions; materialization stays here so the
/// Application layer never references EF Core.
/// </summary>
public interface IAdminRepository<T>
    where T : AdminEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Add(T entity);

    void Remove(T entity);

    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);

    Task<IReadOnlyList<T>> PageAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IOrderedQueryable<T>> order,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IOrderedQueryable<T>> order,
        CancellationToken cancellationToken);
}
