using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ECommercePlatform.Infrastructure.Security;

/// <summary>
/// Wraps ASP.NET Core's <see cref="PasswordHasher{TUser}"/>.
/// </summary>
/// <remarks>
/// We dropped ASP.NET Core Identity's schema, but not its hashing: this is a
/// vetted PBKDF2-HMAC-SHA512 implementation with a per-hash random salt, a
/// versioned format, and a fixed-time comparison. Re-implementing that from
/// primitives would be the single easiest place in this codebase to introduce a
/// subtle, catastrophic bug — so we reuse it and keep only the storage simple.
/// </remarks>
public sealed class PassphraseHasher : IPassphraseHasher
{
    private readonly PasswordHasher<User> _inner;

    public PassphraseHasher()
        => _inner = new PasswordHasher<User>(Microsoft.Extensions.Options.Options.Create(
            new PasswordHasherOptions
            {
                CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,

                // Raised above the framework default. The cost is paid once per
                // login attempt, which also slows offline cracking of a stolen
                // table by the same factor.
                IterationCount = 210_000
            }));

    public string Hash(string passphrase) => _inner.HashPassword(user: null!, passphrase);

    public PassphraseVerificationResult Verify(string hashedPassphrase, string providedPassphrase)
        => _inner.VerifyHashedPassword(user: null!, hashedPassphrase, providedPassphrase) switch
        {
            PasswordVerificationResult.Success => PassphraseVerificationResult.Success,
            PasswordVerificationResult.SuccessRehashNeeded =>
                PassphraseVerificationResult.SuccessRehashNeeded,
            _ => PassphraseVerificationResult.Failed
        };
}
