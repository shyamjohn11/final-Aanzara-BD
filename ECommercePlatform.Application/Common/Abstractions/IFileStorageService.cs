namespace ECommercePlatform.Application.Common.Abstractions;

/// <summary>
/// A file's bytes and descriptive metadata, decoupled from any HTTP type so the
/// Application layer never needs a reference to ASP.NET Core.
/// </summary>
public sealed record FileUpload(Stream Content, string FileName, string ContentType, long Length);

/// <summary>Where an uploaded file ended up after being persisted.</summary>
public sealed record StoredFile(string FilePath, string FileName, string Url, long FileSize, string ContentType);

/// <summary>
/// Persists uploaded files outside the database. Implemented against local disk
/// today; a future implementation (blob storage, S3, ...) can replace it without
/// touching any handler.
/// </summary>
public interface IFileStorageService
{
    Task<StoredFile> SaveAsync(FileUpload file, string subFolder, CancellationToken cancellationToken);

    Task DeleteAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file identified only by its public URL (as returned in
    /// <see cref="StoredFile.Url"/>), for entities that don't persist the
    /// physical FilePath alongside it. A no-op if the URL isn't one this
    /// service produced.
    /// </summary>
    Task DeleteByUrlAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Maps a public file URL back to its physical path, so controllers can
    /// stream the bytes (e.g. banner/subcategory image files). Returns null
    /// when the URL isn't one this service produced. Existence is checked by
    /// the caller, mirroring the category image-file flow.
    /// </summary>
    string? TryResolvePath(string url);
}