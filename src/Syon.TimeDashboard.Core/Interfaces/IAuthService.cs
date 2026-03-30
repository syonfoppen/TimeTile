using Syon.TimeDashboard.Core.Enums;
using Syon.TimeDashboard.Core.Models;

namespace Syon.TimeDashboard.Core.Interfaces;

public interface IAuthService
{
    AuthStatus Status { get; }
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    Task SignOutAsync(CancellationToken cancellationToken = default);
}

public interface IAzureDevOpsAuthService : IAuthService
{
    Task<string> SignInAsync(CancellationToken cancellationToken = default);
}

public interface ISevenPaceAuthService : IAuthService
{
    Task<(string Pin, string Secret)> CreatePinAsync(CancellationToken cancellationToken = default);
    Task<PinPairingStatus> CheckPinStatusAsync(string secret, CancellationToken cancellationToken = default);
    Task<bool> ExchangeSecretForTokenAsync(string secret, CancellationToken cancellationToken = default);
}
