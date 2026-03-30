using Fluxor;
using Microsoft.Extensions.Logging;
using Syon.TimeDashboard.Core.Interfaces;

namespace Syon.TimeDashboard.UI.Store.Timer;

public class TimerEffects
{
    private readonly ITimeTrackingService _trackingService;
    private readonly ILogger<TimerEffects> _logger;

    public TimerEffects(ITimeTrackingService trackingService, ILogger<TimerEffects> logger)
    {
        _trackingService = trackingService;
        _logger = logger;
    }

    [EffectMethod]
    public async Task HandleStartTracking(StartTrackingAction action, IDispatcher dispatcher)
    {
        try
        {
            var session = await _trackingService.StartAsync(action.WorkItemId);
            dispatcher.Dispatch(new TrackingStartedAction(session));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start tracking for work item {Id}", action.WorkItemId);
            dispatcher.Dispatch(new StartTrackingFailedAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleStopTracking(StopTrackingAction action, IDispatcher dispatcher)
    {
        try
        {
            await _trackingService.StopAsync(action.Reason);
            dispatcher.Dispatch(new TrackingStoppedAction());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop tracking");
            dispatcher.Dispatch(new StopTrackingFailedAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleSyncTrackingState(SyncTrackingStateAction _, IDispatcher dispatcher)
    {
        try
        {
            var currentSession = await _trackingService.GetCurrentAsync();
            var recentSessions = await _trackingService.GetLatestAsync(10);
            dispatcher.Dispatch(new TrackingStateSyncedAction(currentSession, recentSessions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync tracking state");
        }
    }
}
