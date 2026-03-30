using Fluxor;

namespace TimeTile.UI.Store.Timer;

public static class TimerReducers
{
    [ReducerMethod]
    public static TimerState OnStartTracking(TimerState state, StartTrackingAction _) =>
        state with { IsStarting = true, ErrorMessage = null };

    [ReducerMethod]
    public static TimerState OnTrackingStarted(TimerState state, TrackingStartedAction action) =>
        state with { ActiveSession = action.Session, IsStarting = false };

    [ReducerMethod]
    public static TimerState OnStartTrackingFailed(TimerState state, StartTrackingFailedAction action) =>
        state with { IsStarting = false, ErrorMessage = action.Error };

    [ReducerMethod]
    public static TimerState OnStopTracking(TimerState state, StopTrackingAction _) =>
        state with { IsStopping = true, ErrorMessage = null };

    [ReducerMethod]
    public static TimerState OnTrackingStopped(TimerState state, TrackingStoppedAction _) =>
        state with { ActiveSession = null, IsStopping = false };

    [ReducerMethod]
    public static TimerState OnStopTrackingFailed(TimerState state, StopTrackingFailedAction action) =>
        state with { IsStopping = false, ErrorMessage = action.Error };

    [ReducerMethod]
    public static TimerState OnTrackingStateSynced(TimerState state, TrackingStateSyncedAction action) =>
        state with { ActiveSession = action.ActiveSession, RecentSessions = action.RecentSessions };
}
