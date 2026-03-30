using Syon.TimeDashboard.Core.Models;

namespace Syon.TimeDashboard.UI.Store.Timer;

public record StartTrackingAction(int WorkItemId);
public record TrackingStartedAction(TrackingSession Session);
public record StartTrackingFailedAction(string Error);

public record StopTrackingAction(int Reason = 0);
public record TrackingStoppedAction;
public record StopTrackingFailedAction(string Error);

public record SyncTrackingStateAction;
public record TrackingStateSyncedAction(TrackingSession? ActiveSession, IReadOnlyList<TrackingSession> RecentSessions);

public record TickTimerAction;
