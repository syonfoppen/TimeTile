using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using TimeTile.Core.Interfaces;

namespace TimeTile.Infrastructure.Security;

[SupportedOSPlatform("windows")]
public sealed class WindowsSecureTokenStore : ISecureTokenStore
{
    private readonly string _storagePath;

    public WindowsSecureTokenStore()
    {
        _storagePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TimeTile", "tokens");
        Directory.CreateDirectory(_storagePath);
    }

    public Task<string?> GetTokenAsync(string key, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(key);
        if (!File.Exists(filePath))
            return Task.FromResult<string?>(null);

        var encryptedBytes = File.ReadAllBytes(filePath);
        var decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
        return Task.FromResult<string?>(Encoding.UTF8.GetString(decryptedBytes));
    }

    public Task SetTokenAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        var plainBytes = Encoding.UTF8.GetBytes(value);
        var encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(GetFilePath(key), encryptedBytes);
        return Task.CompletedTask;
    }

    public Task RemoveTokenAsync(string key, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(key);
        if (File.Exists(filePath))
            File.Delete(filePath);
        return Task.CompletedTask;
    }

    private string GetFilePath(string key)
    {
        var safeKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)))[..16];
        return Path.Combine(_storagePath, $"{safeKey}.bin");
    }
}
