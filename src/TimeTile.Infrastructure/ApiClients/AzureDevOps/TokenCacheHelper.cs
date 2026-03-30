using Microsoft.Identity.Client;

namespace TimeTile.Infrastructure.ApiClients.AzureDevOps;

internal static class TokenCacheHelper
{
    private static readonly string CacheFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TimeTile", "msal_cache.bin");

    private static readonly object FileLock = new();

    public static void EnableSerialization(ITokenCache tokenCache)
    {
        tokenCache.SetBeforeAccess(BeforeAccess);
        tokenCache.SetAfterAccess(AfterAccess);
    }

    private static void BeforeAccess(TokenCacheNotificationArgs args)
    {
        lock (FileLock)
        {
            if (File.Exists(CacheFilePath))
            {
                args.TokenCache.DeserializeMsalV3(File.ReadAllBytes(CacheFilePath));
            }
        }
    }

    private static void AfterAccess(TokenCacheNotificationArgs args)
    {
        if (!args.HasStateChanged)
            return;

        lock (FileLock)
        {
            var dir = Path.GetDirectoryName(CacheFilePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllBytes(CacheFilePath, args.TokenCache.SerializeMsalV3());
        }
    }
}
