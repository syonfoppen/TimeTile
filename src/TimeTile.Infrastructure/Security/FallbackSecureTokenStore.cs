using System.Security.Cryptography;
using System.Text;
using TimeTile.Core.Interfaces;

namespace TimeTile.Infrastructure.Security;

public sealed class FallbackSecureTokenStore : ISecureTokenStore
{
    private readonly string _storagePath;
    private readonly byte[] _entropy;

    public FallbackSecureTokenStore()
    {
        _storagePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TimeTile", "tokens");
        Directory.CreateDirectory(_storagePath);

        // Machine-specific entropy for AES key derivation
        var machineId = Environment.MachineName + Environment.UserName;
        _entropy = SHA256.HashData(Encoding.UTF8.GetBytes(machineId));
    }

    public Task<string?> GetTokenAsync(string key, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(key);
        if (!File.Exists(filePath))
            return Task.FromResult<string?>(null);

        var data = File.ReadAllBytes(filePath);
        if (data.Length < 12 + 16) // IV + minimum tag
            return Task.FromResult<string?>(null);

        var nonce = data[..12];
        var tag = data[12..28];
        var ciphertext = data[28..];

        using var aes = new AesGcm(_entropy[..32], 16);
        var plaintext = new byte[ciphertext.Length];
        aes.Decrypt(nonce, ciphertext, tag, plaintext);
        return Task.FromResult<string?>(Encoding.UTF8.GetString(plaintext));
    }

    public Task SetTokenAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        var plaintext = Encoding.UTF8.GetBytes(value);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(_entropy[..32], 16);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        // Store as: [12 nonce][16 tag][ciphertext]
        var data = new byte[12 + 16 + ciphertext.Length];
        nonce.CopyTo(data, 0);
        tag.CopyTo(data, 12);
        ciphertext.CopyTo(data, 28);

        File.WriteAllBytes(GetFilePath(key), data);
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
        return Path.Combine(_storagePath, $"{safeKey}.enc");
    }
}
