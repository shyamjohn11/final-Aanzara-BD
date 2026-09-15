namespace ECommercePlatform.Application.Common.Abstractions;

public enum PassphraseVerificationResult
{
    Failed,
    Success,

    /// <summary>
    /// Correct, but hashed with outdated parameters. The caller should re-hash and
    /// persist so the stored work factor keeps up with hardware.
    /// </summary>
    SuccessRehashNeeded
}

/// <summary>
/// Hashing of user passphrases. An interface rather than a direct call so the
/// algorithm can be replaced without touching a single handler.
/// </summary>
public interface IPassphraseHasher
{
    string Hash(string passphrase);

    PassphraseVerificationResult Verify(string hashedPassphrase, string providedPassphrase);
}
