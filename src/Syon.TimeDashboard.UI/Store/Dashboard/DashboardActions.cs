using Syon.TimeDashboard.Core.Models;

namespace Syon.TimeDashboard.UI.Store.Dashboard;

public record LoadDashboardAction;
public record DashboardLoadedAction(IReadOnlyList<PinnedTile> Tiles);
public record DashboardLoadFailedAction(string Error);

public record PinWorkItemAction(WorkItem WorkItem);
public record WorkItemPinnedAction(PinnedTile Tile);
public record UnpinWorkItemAction(int WorkItemId);
public record WorkItemUnpinnedAction(int WorkItemId);

public record ReorderTilesAction(IReadOnlyList<int> WorkItemIdsInOrder);
public record TilesReorderedAction(IReadOnlyList<PinnedTile> Tiles);

public record RefreshTilesAction;
public record TilesRefreshedAction(IReadOnlyList<PinnedTile> Tiles);

public record PinMySprintTasksAction;
public record PinMySprintTasksSucceededAction(IReadOnlyList<PinnedTile> NewTiles);
public record PinMySprintTasksFailedAction(string Error);
