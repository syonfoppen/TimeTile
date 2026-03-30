using Fluxor;
using Microsoft.Extensions.Logging;
using TimeTile.Core.Interfaces;

namespace TimeTile.UI.Store.Auth;

public class AuthEffects
{
    private readonly IAzureDevOpsAuthService _azDoAuth;
    private readonly ISevenPaceAuthService _sevenPaceAuth;
    private readonly ILogger<AuthEffects> _logger;

    public AuthEffects(IAzureDevOpsAuthService azDoAuth, ISevenPaceAuthService sevenPaceAuth, ILogger<AuthEffects> logger)
    {
        _azDoAuth = azDoAuth;
        _sevenPaceAuth = sevenPaceAuth;
        _logger = logger;
    }

    [EffectMethod]
    public async Task HandleAzDoSignIn(AzDoSignInAction _, IDispatcher dispatcher)
    {
        try
        {
            var token = await _azDoAuth.SignInAsync();
            dispatcher.Dispatch(new AzDoSignInSuccessAction("Azure DevOps User"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure DevOps sign-in failed");
            dispatcher.Dispatch(new AzDoSignInFailedAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleAzDoSignOut(AzDoSignOutAction _, IDispatcher dispatcher)
    {
        await _azDoAuth.SignOutAsync();
        dispatcher.Dispatch(new AzDoSignOutCompleteAction());
    }

    [EffectMethod]
    public async Task HandleSevenPaceStartPairing(SevenPaceStartPairingAction _, IDispatcher dispatcher)
    {
        try
        {
            var (pin, secret) = await _sevenPaceAuth.CreatePinAsync();
            dispatcher.Dispatch(new SevenPacePinCreatedAction(pin));

            // Poll for validation
            for (var i = 0; i < 120; i++) // 2 minutes max
            {
                await Task.Delay(1000);
                var status = await _sevenPaceAuth.CheckPinStatusAsync(secret);

                if (status == Core.Enums.PinPairingStatus.Validated)
                {
                    dispatcher.Dispatch(new SevenPacePairingValidatedAction());

                    var success = await _sevenPaceAuth.ExchangeSecretForTokenAsync(secret);
                    if (success)
                    {
                        dispatcher.Dispatch(new SevenPaceTokenAcquiredAction(null));
                    }
                    else
                    {
                        dispatcher.Dispatch(new SevenPacePairingFailedAction("Token exchange failed"));
                    }
                    return;
                }
            }

            dispatcher.Dispatch(new SevenPacePairingFailedAction("Pairing timed out. Please try again."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "7pace pairing failed");
            dispatcher.Dispatch(new SevenPacePairingFailedAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleSevenPaceSignOut(SevenPaceSignOutAction _, IDispatcher dispatcher)
    {
        await _sevenPaceAuth.SignOutAsync();
        dispatcher.Dispatch(new SevenPaceSignOutCompleteAction());
    }

    [EffectMethod]
    public async Task HandleRestoreAuthState(RestoreAuthStateAction _, IDispatcher dispatcher)
    {
        var azDoAuth = false;
        string? azDoUser = null;
        var sevenPaceAuth = false;
        string? sevenPaceUser = null;

        try
        {
            // Try silent AzDO token acquisition from MSAL cache
            var token = await _azDoAuth.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                azDoAuth = true;
                azDoUser = "Azure DevOps User";
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "No cached AzDO token available");
        }

        try
        {
            // Try getting existing 7pace token from secure store
            var token = await _sevenPaceAuth.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                sevenPaceAuth = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "No cached 7pace token available");
        }

        dispatcher.Dispatch(new AuthStateRestoredAction(azDoAuth, sevenPaceAuth, azDoUser, sevenPaceUser));

    }

    [EffectMethod]
    public Task HandleSevenPaceTokenAcquired(SevenPaceTokenAcquiredAction _, IDispatcher dispatcher)
    {
        return Task.CompletedTask;
    }
}
