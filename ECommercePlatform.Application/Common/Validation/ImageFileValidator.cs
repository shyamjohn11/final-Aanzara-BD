namespace ECommercePlatform.Application.Common.Validation;

/// <summary>
/// Shared upload checks for image endpoints. Extension allowlist + MIME +
/// magic-byte sniffing so Content-Type alone can never authorize a write.
/// </summary>
public static class ImageFileValidator
{
    public static readonly IReadOnlySet<string> AllowedExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

    private static readonly IReadOnlyDictionary<string, string> AllowedContentTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp",
        };

    public static bool IsAllowedExtension(string? fileNameOrExtension)
    {
        if (string.IsNullOrWhiteSpace(fileNameOrExtension)) return false;
        var ext = fileNameOrExtension.StartsWith('.')
            ? fileNameOrExtension
            : Path.GetExtension(fileNameOrExtension);
        return AllowedExtensions.Contains(ext);
    }

    public static bool IsAllowedContentType(string? contentType)
        => !string.IsNullOrWhiteSpace(contentType) && AllowedContentTypes.ContainsKey(contentType);

    /// <summary>
    /// Validates extension, declared content type, size, and magic bytes.
    /// Returns a human-readable reason when invalid; null when the file is acceptable.
    /// The stream position is restored.
    /// </summary>
    public static async Task<string?> ValidateAsync(
        Stream content,
        string? fileName,
        string? contentType,
        long length,
        long maxLengthBytes,
        CancellationToken cancellationToken)
    {
        if (length <= 0) return "An image file is required.";
        if (maxLengthBytes > 0 && length > maxLengthBytes)
            return $"File exceeds the {maxLengthBytes / (1024 * 1024)} MB limit.";

        if (!IsAllowedExtension(fileName))
            return "Only .jpg, .jpeg, .png, and .webp images are allowed.";

        if (!IsAllowedContentType(contentType))
            return "Only image/jpeg, image/png, and image/webp content types are allowed.";

        var original = content.CanSeek ? content.Position : 0;
        try
        {
            var header = new byte[12];
            var read = 0;
            while (read < header.Length)
            {
                var n = await content.ReadAsync(header.AsMemory(read, header.Length - read), cancellationToken);
                if (n == 0) break;
                read += n;
            }

            if (!MatchesMagicBytes(header.AsSpan(0, read), contentType!))
                return "File content does not match an allowed image format.";
        }
        finally
        {
            if (content.CanSeek) content.Position = original;
        }

        return null;
    }

    private static bool MatchesMagicBytes(ReadOnlySpan<byte> header, string contentType)
    {
        if (header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            return true; // JPEG

        if (header.Length >= 8 &&
            header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
            header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
            return true; // PNG

        if (header.Length >= 12 &&
            header[0] == (byte)'R' && header[1] == (byte)'I' && header[2] == (byte)'F' && header[3] == (byte)'F' &&
            header[8] == (byte)'W' && header[9] == (byte)'E' && header[10] == (byte)'B' && header[11] == (byte)'P')
            return true; // WEBP (RIFF....WEBP)

        // Declared type mismatch is already a fail; reject anything else.
        return false;
    }

    public static string SafeExtensionFor(string? contentType)
    {
        if (!string.IsNullOrWhiteSpace(contentType) && AllowedContentTypes.TryGetValue(contentType, out var ext))
            return ext;
        return ".jpg";
    }
}
