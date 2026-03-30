namespace Syon.TimeDashboard.Core.Interfaces;

public interface ISecureTokenStore
{
    Task<string?> GetTokenAsync(string key, CancellationToken cancellationToken = default);
    Task SetTokenAsync(string key, string value, CancellationToken cancellationToken = default);
    Task RemoveTokenAsync(string key, CancellationToken cancellationToken = default);
}
