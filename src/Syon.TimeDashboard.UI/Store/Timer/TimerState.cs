using Fluxor;
using Syon.TimeDashboard.Core.Models;

namespace Syon.TimeDashboard.UI.Store.Timer;

[FeatureState]
public record TimerState
{
    public TrackingSession? ActiveSession { get; init; }
    public bool IsStarting { get; init; }
    public bool IsStopping { get; init; }
    public IReadOnlyList<TrackingSession> RecentSessions { get; init; } = [];
    public string? ErrorMessage { get; init; }
}
