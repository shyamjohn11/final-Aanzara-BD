using Microsoft.Net.Http.Headers;

namespace ECommercePlatform.Api.Extensions;

/// <summary>
/// Image files stream from a fixed URL (…/image/file) whose content changes when an
/// image is re-uploaded, so responses must be revalidated on every use instead of
/// being heuristically cached by the browser (stale image after edit otherwise).
/// </summary>
public static class FileResponseExtensions
{
    public static void SetImageRevalidationCacheHeaders(this HttpResponse response)
        => response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue { NoCache = true };
}
