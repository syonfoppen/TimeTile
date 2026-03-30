using System.Net;
using System.Net.Http.Headers;

namespace TimeTile.Infrastructure.ApiClients.SevenPace;

public sealed class SevenPaceAuthHandler : DelegatingHandler
{
    private readonly SevenPaceAuthService _authService;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public SevenPaceAuthHandler(SevenPaceAuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await AttachTokenAsync(request, cancellationToken);
        var response = await base.SendAsync(request, cancellationToken);



        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await _refreshLock.WaitAsync(cancellationToken);
            try
            {
                // Retry with fresh token
                await AttachTokenAsync(request, cancellationToken);
                response = await base.SendAsync(request, cancellationToken);
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        return response;
    }

    private async Task AttachTokenAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _authService.GetAccessTokenAsync(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        catch
        {
            // No token available — let the request go through unauthenticated
        }
    }
}
