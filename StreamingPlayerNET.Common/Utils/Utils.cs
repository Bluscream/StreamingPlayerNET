using System.Web;

namespace StreamingPlayerNET.Common.Utils;

public static class UrlUtils
{
    /// <summary>
    /// Decodes a URL-encoded string to fix "An invalid request URI was provided" errors.
    /// This is needed when URLs come from external APIs that may be double-encoded.
    /// </summary>
    /// <param name="url">The URL-encoded string to decode</param>
    /// <returns>The decoded URL string</returns>
    public static string DecodeUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return url;
            
        return HttpUtility.UrlDecode(url);
    }
    
    /// <summary>
    /// Decodes a URL-encoded string and logs the transformation for debugging.
    /// </summary>
    /// <param name="url">The URL-encoded string to decode</param>
    /// <param name="logger">Optional logger to record the transformation</param>
    /// <returns>The decoded URL string</returns>
    public static string DecodeUrlWithLogging(string url, NLog.Logger? logger = null)
    {
        if (string.IsNullOrEmpty(url))
            return url;
            
        var decodedUrl = HttpUtility.UrlDecode(url);
        
        logger?.Debug($"URL decoding: '{url}' -> '{decodedUrl}'");
        
        return decodedUrl;
    }
}
