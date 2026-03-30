using TimeTile.Core.Models;

namespace TimeTile.Core.Interfaces;

public interface ITimeTrackingService
{
    Task<TrackingSession?> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<TrackingSession> StartAsync(int workItemId, CancellationToken cancellationToken = default);
    Task StopAsync(int reason = 0, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrackingSession>> GetLatestAsync(int count = 10, CancellationToken cancellationToken = default);
}
