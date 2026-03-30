using Fluxor;

namespace Syon.TimeDashboard.UI.Store.Dashboard;

public static class DashboardReducers
{
    [ReducerMethod]
    public static DashboardState OnLoadDashboard(DashboardState state, LoadDashboardAction _) =>
        state with { IsLoading = true, ErrorMessage = null };

    [ReducerMethod]
    public static DashboardState OnDashboardLoaded(DashboardState state, DashboardLoadedAction action) =>
        state with { Tiles = action.Tiles, IsLoading = false };

    [ReducerMethod]
    public static DashboardState OnDashboardLoadFailed(DashboardState state, DashboardLoadFailedAction action) =>
        state with { IsLoading = false, ErrorMessage = action.Error };

    [ReducerMethod]
    public static DashboardState OnWorkItemPinned(DashboardState state, WorkItemPinnedAction action) =>
        state with { Tiles = [.. state.Tiles, action.Tile] };

    [ReducerMethod]
    public static DashboardState OnWorkItemUnpinned(DashboardState state, WorkItemUnpinnedAction action) =>
        state with { Tiles = state.Tiles.Where(t => t.WorkItemId != action.WorkItemId).ToList() };

    [ReducerMethod]
    public static DashboardState OnTilesReordered(DashboardState state, TilesReorderedAction action) =>
        state with { Tiles = action.Tiles };

    [ReducerMethod]
    public static DashboardState OnTilesRefreshed(DashboardState state, TilesRefreshedAction action) =>
        state with { Tiles = action.Tiles };

    [ReducerMethod]
    public static DashboardState OnPinMySprintTasks(DashboardState state, PinMySprintTasksAction _) =>
        state with { IsPinningSprintTasks = true };

    [ReducerMethod]
    public static DashboardState OnPinMySprintTasksSucceeded(DashboardState state, PinMySprintTasksSucceededAction action) =>
        state with { IsPinningSprintTasks = false, Tiles = [.. state.Tiles, .. action.NewTiles] };

    [ReducerMethod]
    public static DashboardState OnPinMySprintTasksFailed(DashboardState state, PinMySprintTasksFailedAction action) =>
        state with { IsPinningSprintTasks = false, ErrorMessage = action.Error };
}
