namespace Syon.TimeDashboard.UI.Store.Auth;

public record AzDoSignInAction;
public record AzDoSignInSuccessAction(string UserName);
public record AzDoSignInFailedAction(string Error);
public record AzDoSignOutAction;
public record AzDoSignOutCompleteAction;

public record SevenPaceStartPairingAction;
public record SevenPacePinCreatedAction(string Pin);
public record SevenPacePairingValidatedAction;
public record SevenPaceTokenAcquiredAction(string? UserName);
public record SevenPacePairingFailedAction(string Error);
public record SevenPaceSignOutAction;
public record SevenPaceSignOutCompleteAction;

public record RestoreAuthStateAction;
public record AuthStateRestoredAction(bool AzDoAuthenticated, bool SevenPaceAuthenticated, string? AzDoUser, string? SevenPaceUser);
