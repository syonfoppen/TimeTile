using Fluxor;
using Syon.TimeDashboard.Core.Enums;

namespace Syon.TimeDashboard.UI.Store.Auth;

[FeatureState]
public record AuthState
{
    public AuthStatus AzDoStatus { get; init; } = AuthStatus.NotAuthenticated;
    public AuthStatus SevenPaceStatus { get; init; } = AuthStatus.NotAuthenticated;
    public string? AzDoUserName { get; init; }
    public string? SevenPaceUserName { get; init; }
    public string? PairingPin { get; init; }
    public PinPairingStatus PairingStatus { get; init; } = PinPairingStatus.NotStarted;
    public string? ErrorMessage { get; init; }
}
