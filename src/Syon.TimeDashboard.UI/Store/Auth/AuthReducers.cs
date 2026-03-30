using Fluxor;
using Syon.TimeDashboard.Core.Enums;

namespace Syon.TimeDashboard.UI.Store.Auth;

public static class AuthReducers
{
    [ReducerMethod]
    public static AuthState OnAzDoSignIn(AuthState state, AzDoSignInAction _) =>
        state with { AzDoStatus = AuthStatus.Authenticating, ErrorMessage = null };

    [ReducerMethod]
    public static AuthState OnAzDoSignInSuccess(AuthState state, AzDoSignInSuccessAction action) =>
        state with { AzDoStatus = AuthStatus.Authenticated, AzDoUserName = action.UserName };

    [ReducerMethod]
    public static AuthState OnAzDoSignInFailed(AuthState state, AzDoSignInFailedAction action) =>
        state with { AzDoStatus = AuthStatus.Failed, ErrorMessage = action.Error };

    [ReducerMethod]
    public static AuthState OnAzDoSignOutComplete(AuthState state, AzDoSignOutCompleteAction _) =>
        state with { AzDoStatus = AuthStatus.NotAuthenticated, AzDoUserName = null };

    [ReducerMethod]
    public static AuthState OnSevenPaceStartPairing(AuthState state, SevenPaceStartPairingAction _) =>
        state with { SevenPaceStatus = AuthStatus.Authenticating, PairingStatus = PinPairingStatus.NotStarted, PairingPin = null, ErrorMessage = null };

    [ReducerMethod]
    public static AuthState OnSevenPacePinCreated(AuthState state, SevenPacePinCreatedAction action) =>
        state with { PairingPin = action.Pin, PairingStatus = PinPairingStatus.PinCreated };

    [ReducerMethod]
    public static AuthState OnSevenPacePairingValidated(AuthState state, SevenPacePairingValidatedAction _) =>
        state with { PairingStatus = PinPairingStatus.Validated };

    [ReducerMethod]
    public static AuthState OnSevenPaceTokenAcquired(AuthState state, SevenPaceTokenAcquiredAction action) =>
        state with { SevenPaceStatus = AuthStatus.Authenticated, PairingStatus = PinPairingStatus.TokenAcquired, SevenPaceUserName = action.UserName, PairingPin = null };

    [ReducerMethod]
    public static AuthState OnSevenPacePairingFailed(AuthState state, SevenPacePairingFailedAction action) =>
        state with { SevenPaceStatus = AuthStatus.Failed, PairingStatus = PinPairingStatus.Failed, ErrorMessage = action.Error, PairingPin = null };

    [ReducerMethod]
    public static AuthState OnSevenPaceSignOutComplete(AuthState state, SevenPaceSignOutCompleteAction _) =>
        state with { SevenPaceStatus = AuthStatus.NotAuthenticated, SevenPaceUserName = null, PairingPin = null, PairingStatus = PinPairingStatus.NotStarted };

    [ReducerMethod]
    public static AuthState OnAuthStateRestored(AuthState state, AuthStateRestoredAction action) =>
        state with
        {
            AzDoStatus = action.AzDoAuthenticated ? AuthStatus.Authenticated : AuthStatus.NotAuthenticated,
            SevenPaceStatus = action.SevenPaceAuthenticated ? AuthStatus.Authenticated : AuthStatus.NotAuthenticated,
            AzDoUserName = action.AzDoUser,
            SevenPaceUserName = action.SevenPaceUser
        };
}
