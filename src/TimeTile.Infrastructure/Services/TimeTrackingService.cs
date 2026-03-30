using TimeTile.Core.Interfaces;
using TimeTile.Core.Models;
using TimeTile.Infrastructure.ApiClients.SevenPace;

namespace TimeTile.Infrastructure.Services;

public sealed class TimeTrackingService : ITimeTrackingService
{
    private readonly SevenPaceApiClient _apiClient;

    public TimeTrackingService(SevenPaceApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<TrackingSession?> GetCurrentAsync(CancellationToken cancellationToken = default)
        => _apiClient.GetCurrentTrackingAsync(cancellationToken);

    public Task<TrackingSession> StartAsync(int workItemId, CancellationToken cancellationToken = default)
        => _apiClient.StartTrackingAsync(workItemId, cancellationToken);

    public Task StopAsync(int reason = 0, CancellationToken cancellationToken = default)
        => _apiClient.StopTrackingAsync(reason, cancellationToken);

    public Task<IReadOnlyList<TrackingSession>> GetLatestAsync(int count = 10, CancellationToken cancellationToken = default)
        => _apiClient.GetLatestTracksAsync(count, cancellationToken);
}
