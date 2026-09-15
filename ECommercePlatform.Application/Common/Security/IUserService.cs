using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Common.Security;

/// <summary>
/// Defines all user account management operations for the application layer.
/// Each method returns a Result to indicate success or a specific domain error.
/// </summary>
public interface IUserService
{
    /// <summary>Retrieves every registered user account.</summary>
    Task<IReadOnlyList<User>> GetAllUsersAsync(CancellationToken cancellationToken);

    /// <summary>Finds a single user by their unique identifier.</summary>
    Task<User?> FindUserByIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a brand-new user account with the provided name, email, and password.
    /// Returns the new user's identifier on success, or an error if the email is already taken.
    /// </summary>
    Task<Result<Guid>> CreateNewUserAsync(
        string name, string email, string passphrase, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing user's name, email, phone, and status.
    /// Returns an error if the user does not exist.
    /// </summary>
    Task<Result> UpdateExistingUserAsync(
        Guid userId, string name, string email, string? phone, string status, CancellationToken cancellationToken);

    /// <summary>
    /// Permanently removes a user account.
    /// Returns an error if the user does not exist.
    /// </summary>
    Task<Result> DeleteUserAsync(Guid userId, CancellationToken cancellationToken);
}