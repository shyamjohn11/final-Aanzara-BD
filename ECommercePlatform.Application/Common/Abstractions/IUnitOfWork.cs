namespace ECommercePlatform.Application.Common.Abstractions;

/// <summary>
/// The transaction boundary. Repositories stage changes; a handler commits them
/// exactly once, so one use case is one atomic write.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
