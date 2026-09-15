using ECommercePlatform.Domain.Entities;

namespace ECommercePlatform.Application.Common.Abstractions;

/// <summary>
/// Defines all data-access operations for user accounts.
/// Each method maps directly to a database operation on the USERS table.
/// </summary>
public interface IUserRepository
{
    /// <summary>Retrieves every registered user account, ordered by name then email.</summary>
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>Finds a single user by their unique identifier.</summary>
    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Looks up a user by email (case-insensitive in the database).</summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>
    /// Looks up a user by either their email or their registered mobile
    /// number. Login accepts a single "email or mobile" field, so this
    /// resolves whichever identifier the caller submitted.
    /// </summary>
    Task<User?> GetByEmailOrPhoneAsync(string identifier, CancellationToken cancellationToken);

    /// <summary>Checks whether a given email address is already registered.</summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

    /// <summary>Adds a new user to the database.</summary>
    void Add(User user);

    /// <summary>Updates an existing user in the database.</summary>
    void Update(User user);

    /// <summary>Removes a user from the database.</summary>
    void Delete(User user);

    /// <summary>Adds a new user to the database and saves changes.</summary>
    Task AddAsync(User user, CancellationToken cancellationToken);

    /// <summary>Updates an existing user in the database and saves changes.</summary>
    Task UpdateAsync(User user, CancellationToken cancellationToken);

    /// <summary>Removes a user from the database and saves changes.</summary>
    Task DeleteAsync(User user, CancellationToken cancellationToken);
}