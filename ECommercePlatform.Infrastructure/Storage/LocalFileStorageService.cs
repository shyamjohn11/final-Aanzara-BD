using ECommercePlatform.Application.Common.Abstractions;
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
        var folder = Path.Combine(_options.RootPath, subFolder);
        Directory.CreateDirectory(folder);

        // GUID filename avoids collisions and sidesteps anything unsafe in the
        // caller-supplied name; the original extension is kept as a content hint.
        var extension = Path.GetExtension(file.FileName);
        var storedName = $"{Guid.NewGuid()}{extension}";
                var fullPath = Path.GetFullPath(Path.Combine(folder, storedName));

        await using (var destination = File.Create(fullPath))
        {
            await file.Content.CopyToAsync(destination, cancellationToken);
        }

        var url = $"{_options.PublicBaseUrl.TrimEnd('/')}/{subFolder}/{storedName}";

        return new StoredFile(fullPath, storedName, url, file.Length, file.ContentType);
    }

    public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
        {
            File.Delete(filePath);
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

        var basePrefix = $"{_options.PublicBaseUrl.TrimEnd('/')}/";

        if (!url.StartsWith(basePrefix, StringComparison.OrdinalIgnoreCase))
        {
            // Not a URL this service produced (e.g. left over from a different
            // storage backend, or an external http URL); nothing we can resolve.
            // Same-host relative URLs resolve against RootPath, using the path
            // segment from PublicBaseUrl ("/uploads/…" in the default config).
            var basePath = Uri.TryCreate(
                    _options.PublicBaseUrl, UriKind.Absolute, out var baseUri)
                ? baseUri.AbsolutePath.Trim('/')
                : "uploads";

            var relativePrefix = $"/{basePath}/";

            if (string.IsNullOrWhiteSpace(basePath)
                || !url.StartsWith(relativePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var appRelative = url[relativePrefix.Length..].Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(_options.RootPath, appRelative);
        }

        var relative = url[basePrefix.Length..].Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(_options.RootPath, relative);
    }
}