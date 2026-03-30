using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Syon.TimeDashboard.Core.Enums;
using Syon.TimeDashboard.Core.Interfaces;

namespace Syon.TimeDashboard.Infrastructure.ApiClients.SevenPace;

public sealed class SevenPaceAuthService : ISevenPaceAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISecureTokenStore _tokenStore;
    private const string HttpClientName = "SevenPaceAuth";
    private const string AccessTokenKey = "7pace_access_token";
    private const string RefreshTokenKey = "7pace_refresh_token";

    public AuthStatus Status { get; private set; } = AuthStatus.NotAuthenticated;

    public SevenPaceAuthService(IHttpClientFactory httpClientFactory, ISecureTokenStore tokenStore)
    {
        _httpClientFactory = httpClientFactory;
        _tokenStore = tokenStore;
    }

    public async Task<(string Pin, string Secret)> CreatePinAsync(CancellationToken cancellationToken = default)
    {
        using var httpClient = _httpClientFactory.CreateClient(HttpClientName);
        var response = await httpClient.PostAsync("api/pin/create?api-version=3.2", null, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PinCreateResponse>(cancellationToken: cancellationToken);
        return (result!.Pin, result.Secret);
    }

    public async Task<PinPairingStatus> CheckPinStatusAsync(string secret, CancellationToken cancellationToken = default)
    {
        using var httpClient = _httpClientFactory.CreateClient(HttpClientName);
        var response = await httpClient.PostAsJsonAsync("api/pin/status?api-version=3.2", secret, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PinStatusResponse>(cancellationToken: cancellationToken);
        return result?.Status?.ToLowerInvariant() switch
        {
            "validated" => PinPairingStatus.Validated,
            "created" => PinPairingStatus.WaitingForValidation,
            _ => PinPairingStatus.WaitingForValidation
        };
    }

    public async Task<bool> ExchangeSecretForTokenAsync(string secret, CancellationToken cancellationToken = default)
    {
        Status = AuthStatus.Authenticating;
        try
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = "OpenApi",
                ["grant_type"] = "authorization_code",
                ["code"] = secret
            });

            using var httpClient = _httpClientFactory.CreateClient(HttpClientName);
            var response = await httpClient.PostAsync("token", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);
            if (tokenResponse is null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                Status = AuthStatus.Failed;
                return false;
            }

            await _tokenStore.SetTokenAsync(AccessTokenKey, tokenResponse.AccessToken, cancellationToken);
            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
                await _tokenStore.SetTokenAsync(RefreshTokenKey, tokenResponse.RefreshToken, cancellationToken);

            Status = AuthStatus.Authenticated;
            return true;
        }
        catch
        {
            Status = AuthStatus.Failed;
            return false;
        }
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var token = await _tokenStore.GetTokenAsync(AccessTokenKey, cancellationToken);
        if (!string.IsNullOrEmpty(token))
            return token;

        // Try refresh
        var refreshToken = await _tokenStore.GetTokenAsync(RefreshTokenKey, cancellationToken);
        if (string.IsNullOrEmpty(refreshToken))
            throw new InvalidOperationException("No 7pace token available. Please re-authenticate.");

        var refreshed = await RefreshTokenAsync(refreshToken, cancellationToken);
        if (!refreshed)
            throw new InvalidOperationException("7pace token refresh failed. Please re-authenticate.");

        return (await _tokenStore.GetTokenAsync(AccessTokenKey, cancellationToken))!;
    }

    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        await _tokenStore.RemoveTokenAsync(AccessTokenKey, cancellationToken);
        await _tokenStore.RemoveTokenAsync(RefreshTokenKey, cancellationToken);
        Status = AuthStatus.NotAuthenticated;
    }

    internal async Task<bool> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = "OpenApi",
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken
            });

            using var httpClient = _httpClientFactory.CreateClient(HttpClientName);
            var response = await httpClient.PostAsync("token", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);
            if (tokenResponse is null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                return false;

            await _tokenStore.SetTokenAsync(AccessTokenKey, tokenResponse.AccessToken, cancellationToken);
            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
                await _tokenStore.SetTokenAsync(RefreshTokenKey, tokenResponse.RefreshToken, cancellationToken);

            Status = AuthStatus.Authenticated;
            return true;
        }
        catch
        {
            Status = AuthStatus.Failed;
            return false;
        }
    }

    private sealed record PinCreateResponse
    {
        [JsonPropertyName("pin")] public string Pin { get; init; } = "";
        [JsonPropertyName("secret")] public string Secret { get; init; } = "";
    }

    private sealed record PinStatusResponse
    {
        [JsonPropertyName("status")] public string Status { get; init; } = "";
    }

    private sealed record TokenResponse
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; init; } = "";
        [JsonPropertyName("refresh_token")] public string RefreshToken { get; init; } = "";
    }
}
