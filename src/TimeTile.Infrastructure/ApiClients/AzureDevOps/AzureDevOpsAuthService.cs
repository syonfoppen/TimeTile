using Microsoft.Identity.Client;
using TimeTile.Core.Enums;
using TimeTile.Core.Interfaces;

namespace TimeTile.Infrastructure.ApiClients.AzureDevOps;

public sealed class AzureDevOpsAuthService : IAzureDevOpsAuthService
{
    private readonly IPublicClientApplication _pca;
    private readonly string[] _scopes = ["499b84ac-1321-427f-aa17-267ca6975798/.default"];

    public AuthStatus Status { get; private set; } = AuthStatus.NotAuthenticated;

    public AzureDevOpsAuthService(string clientId, string tenantId)
    {
        var builder = PublicClientApplicationBuilder
            .Create(clientId)
            .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
            .WithDefaultRedirectUri();

#if WINDOWS
        builder = builder.WithBroker(new BrokerOptions(BrokerOptions.OperatingSystems.Windows));
#endif

        _pca = builder.Build();
        TokenCacheHelper.EnableSerialization(_pca.UserTokenCache);
    }

    public async Task<string> SignInAsync(CancellationToken cancellationToken = default)
    {
        Status = AuthStatus.Authenticating;
        try
        {
            var result = await AcquireTokenAsync(cancellationToken);
            Status = AuthStatus.Authenticated;
            return result.Account?.Username ?? "Unknown";
        }
        catch
        {
            Status = AuthStatus.Failed;
            throw;
        }
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var result = await AcquireTokenAsync(cancellationToken);
        return result.AccessToken;
    }

    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        var accounts = await _pca.GetAccountsAsync();
        foreach (var account in accounts)
        {
            await _pca.RemoveAsync(account);
        }
        Status = AuthStatus.NotAuthenticated;
    }

    private async Task<AuthenticationResult> AcquireTokenAsync(CancellationToken cancellationToken)
    {
        var accounts = await _pca.GetAccountsAsync();
        var firstAccount = accounts.FirstOrDefault();

        try
        {
            return await _pca
                .AcquireTokenSilent(_scopes, firstAccount)
                .ExecuteAsync(cancellationToken);
        }
        catch (MsalUiRequiredException)
        {
            return await _pca
                .AcquireTokenInteractive(_scopes)
                .WithAccount(firstAccount)
                .ExecuteAsync(cancellationToken);
        }
    }
}
