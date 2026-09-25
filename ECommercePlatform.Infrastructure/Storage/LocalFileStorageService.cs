using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Validation;
using Microsoft.Extensions.Options;

namespace ECommercePlatform.Infrastructure.Storage;

/// <summary>Writes uploaded files to a configured folder on local disk.</summary>
public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageOptions _options;

    public LocalFileStorageService(IOptions<FileStorageOptions> options) => _options = options.Value;

    public async Task<StoredFile> SaveAsync(
        FileUpload file, string subFolder, CancellationToken cancellationToken)
    {
        // Only allow known leaf folders — never accept path segments from callers.
        var safeFolder = Path.GetFileName(subFolder);
        if (string.IsNullOrWhiteSpace(safeFolder) || safeFolder != subFolder)
        {
            throw new InvalidOperationException("Invalid upload sub-folder.");
        }

        var folder = Path.GetFullPath(Path.Combine(_options.RootPath, safeFolder));
        var root = Path.GetFullPath(_options.RootPath);
        if (!folder.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Upload path escaped the storage root.");
        }

        Directory.CreateDirectory(folder);

        // Server-generated name only. Extension comes from validated content-type
        // (or an allowlisted original extension), never from a free-form client name.
        var extension = ImageFileValidator.IsAllowedExtension(file.FileName)
            ? Path.GetExtension(file.FileName)
            : ImageFileValidator.SafeExtensionFor(file.ContentType);
        if (!ImageFileValidator.AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Disallowed file extension.");
        }

        var storedName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.GetFullPath(Path.Combine(folder, storedName));
        if (!fullPath.StartsWith(folder, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Upload path escaped the storage root.");
        }

        await using (var destination = File.Create(fullPath))
        {
            await file.Content.CopyToAsync(destination, cancellationToken);
        }

        var url = $"{_options.PublicBaseUrl.TrimEnd('/')}/{safeFolder}/{storedName}";

        return new StoredFile(fullPath, storedName, url, file.Length, file.ContentType ?? "application/octet-stream");
    }

    public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return Task.CompletedTask;

        var full = Path.GetFullPath(filePath);
        var root = Path.GetFullPath(_options.RootPath);
        if (full.StartsWith(root, StringComparison.OrdinalIgnoreCase) && File.Exists(full))
        {
            File.Delete(full);
        }

        return Task.CompletedTask;
    }

    public Task DeleteByUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        var fullPath = TryResolvePath(url);

        if (fullPath is not null && File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public string? TryResolvePath(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        // Reject backslashes and encoded traversal before combining paths.
        if (url.Contains('\\'))
        {
            return null;
        }

        // Absolute URL (legacy PublicBaseUrl): keep only the path. Host no
        // longer matters — /uploads/... is rewritten/served same-origin.
        if (url.Contains("://", StringComparison.Ordinal))
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var absUri))
            {
                return null;
            }

            url = absUri.AbsolutePath;
        }

        var basePrefix = $"{_options.PublicBaseUrl.TrimEnd('/')}/";
        string relative;

        if (url.StartsWith(basePrefix, StringComparison.OrdinalIgnoreCase))
        {
            relative = url[basePrefix.Length..];
        }
        else
        {
            var basePath = Uri.TryCreate(
                    _options.PublicBaseUrl, UriKind.Absolute, out var baseUri2)
                ? baseUri2.AbsolutePath.Trim('/')
                : "uploads";

            var relativePrefix = $"/{basePath}/";
            if (string.IsNullOrWhiteSpace(basePath)
                || !url.StartsWith(relativePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            relative = url[relativePrefix.Length..];
        }

        // Normalize and confirm the result stays under RootPath (no ../ escape).
        var appRelative = relative.Replace('/', Path.DirectorySeparatorChar);
        if (appRelative.Contains(".."))
        {
            return null;
        }

        var root = Path.GetFullPath(_options.RootPath);
        var candidate = Path.GetFullPath(Path.Combine(root, appRelative));
        if (!candidate.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return candidate;
    }
}
