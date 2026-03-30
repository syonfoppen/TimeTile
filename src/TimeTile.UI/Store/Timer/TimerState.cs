using Fluxor;
using TimeTile.Core.Models;

namespace TimeTile.UI.Store.Timer;

[FeatureState]
public record TimerState
{
    public TrackingSession? ActiveSession { get; init; }
    public bool IsStarting { get; init; }
    public bool IsStopping { get; init; }
    public IReadOnlyList<TrackingSession> RecentSessions { get; init; } = [];
    public string? ErrorMessage { get; init; }
}
