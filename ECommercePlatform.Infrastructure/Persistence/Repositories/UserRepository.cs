using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Persistence.Repositories;

/// <summary>
/// Concrete implementation of IUserRepository using Entity Framework Core.
/// Each method performs the corresponding database query or command.
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _database;

    public UserRepository(ApplicationDbContext database) => _database = database;

    /// <summary>Retrieves all users ordered alphabetically by name, then email.</summary>
    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken)
        => await _database.Users
            .AsNoTracking()
            .OrderBy(user => user.Name)
            .ThenBy(user => user.Email)
            .ToListAsync(cancellationToken);

    /// <summary>Finds a single user by their unique identifier.</summary>
    public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
        => _database.Users.FirstOrDefaultAsync(user => user.UserId == userId, cancellationToken);

    /// <summary>Finds a user by email using the database's case-insensitive collation.</summary>
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        => _database.Users.FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

    /// <summary>
    /// Finds a user by email or by the exact registered mobile number, so the
    /// single "email or mobile" login field works for both identifier types.
    /// </summary>
    public Task<User?> GetByEmailOrPhoneAsync(string identifier, CancellationToken cancellationToken)
        => _database.Users.FirstOrDefaultAsync(
            user => user.Email == identifier || user.Phone == identifier,
            cancellationToken);

    /// <summary>Checks if a given email is already present in the database.</summary>
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
        => _database.Users.AnyAsync(user => user.Email == email, cancellationToken);

    /// <summary>Adds a new user row to the database.</summary>
    public void Add(User user) => _database.Users.Add(user);

    /// <summary>Updates an existing user row in the database.</summary>
    public void Update(User user) => _database.Users.Update(user);

    /// <summary>Removes a user row from the database.</summary>
    public void Delete(User user) => _database.Users.Remove(user);

    /// <summary>Adds a new user row and saves changes to the database.</summary>
    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await _database.Users.AddAsync(user, cancellationToken);
        await _database.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Updates an existing user row and saves changes to the database.</summary>
    public async Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        _database.Users.Update(user);
        await _database.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Removes a user row and saves changes to the database.</summary>
    public async Task DeleteAsync(User user, CancellationToken cancellationToken)
    {
        _database.Users.Remove(user);
        await _database.SaveChangesAsync(cancellationToken);
    }
}