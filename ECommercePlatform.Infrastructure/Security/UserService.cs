using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Security;
using ECommercePlatform.Application.Features.Auth;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Entities;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Infrastructure.Security;

/// <summary>
/// Handles all user account operations: listing, retrieving, creating, updating, and deleting.
/// Coordinates the user repository for data access and the role repository for role assignment.
/// Passwords are hashed through IPassphraseHasher before being stored.
/// New accounts are automatically assigned the Customer role.
/// </summary>
public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPassphraseHasher _passphraseHasher;
    private readonly IRoleRepository _roleRepository;

    public UserService(
        IUserRepository userRepository,
        IPassphraseHasher passphraseHasher,
        IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _passphraseHasher = passphraseHasher;
        _roleRepository = roleRepository;
    }

    /// <summary>Retrieves all registered user accounts.</summary>
    public Task<IReadOnlyList<User>> GetAllUsersAsync(CancellationToken cancellationToken)
        => _userRepository.GetAllAsync(cancellationToken);

    /// <summary>Finds a single user by their unique identifier.</summary>
    public async Task<User?> FindUserByIdAsync(Guid userId, CancellationToken cancellationToken)
        => await _userRepository.GetByIdAsync(userId, cancellationToken);

    /// <summary>
    /// Creates a new user account. The provided password is hashed before storage.
    /// If the email is already registered, the operation fails.
    /// The new user is automatically assigned the Customer role.
    /// </summary>
    public async Task<Result<Guid>> CreateNewUserAsync(
        string name, string email, string passphrase, CancellationToken cancellationToken)
    {
        var trimmedEmail = email.Trim();

        if (await _userRepository.EmailExistsAsync(trimmedEmail, cancellationToken))
        {
            return Result.Failure<Guid>(AuthErrors.EmailAlreadyRegistered);
        }

        var newUser = new User
        {
            UserId = Guid.NewGuid(),
            Name = name.Trim(),
            Email = trimmedEmail,
            HashedPassphrase = _passphraseHasher.Hash(passphrase),
            Status = UserStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.AddAsync(newUser, cancellationToken);

        await AssignCustomerRoleToUserAsync(newUser, cancellationToken);

        return Result.Success(newUser.UserId);
    }

    /// <summary>
    /// Updates an existing user's name, email, phone, and status.
    /// Returns an error if the user cannot be found.
    /// </summary>
    public async Task<Result> UpdateExistingUserAsync(
        Guid userId, string name, string email, string? phone, string status, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (existingUser is null)
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        existingUser.Name = name.Trim();
        existingUser.Email = email.Trim();
        existingUser.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        existingUser.Status = status;
        existingUser.UpdatedAt = DateTimeOffset.UtcNow;

        await _userRepository.UpdateAsync(existingUser, cancellationToken);

        return Result.Success();
    }

    /// <summary>
    /// Permanently removes a user account.
    /// Returns an error if the user cannot be found.
    /// </summary>
    public async Task<Result> DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (existingUser is null)
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        await _userRepository.DeleteAsync(existingUser, cancellationToken);

        return Result.Success();
    }

    /// <summary>
    /// Finds the Customer role in the database and assigns it to the newly created user.
    /// If the Customer role does not exist, the assignment is skipped silently.
    /// </summary>
    private async Task AssignCustomerRoleToUserAsync(
        User newUser, CancellationToken cancellationToken)
    {
        var allRoles = await _roleRepository.GetAllAsync(cancellationToken);
        var customerRole = allRoles.FirstOrDefault(role => role.RoleName == Roles.Customer);

        if (customerRole is not null)
        {
            _roleRepository.AddAdminUserRole(newUser.UserId, customerRole.RoleId);
        }
    }
}