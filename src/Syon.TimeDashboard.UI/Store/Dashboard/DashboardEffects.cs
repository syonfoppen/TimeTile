using Fluxor;
using Microsoft.Extensions.Logging;
using Syon.TimeDashboard.Core.Interfaces;
using Syon.TimeDashboard.Core.Models;
using Syon.TimeDashboard.UI.Store.Filter;

namespace Syon.TimeDashboard.UI.Store.Dashboard;

public class DashboardEffects
{
    private readonly IDashboardRepository _repository;
    private readonly IWorkItemService _workItemService;
    private readonly IState<FilterState> _filterState;
    private readonly ISettingsRepository _settingsRepository;
    private readonly ILogger<DashboardEffects> _logger;

    public DashboardEffects(
        IDashboardRepository repository,
        IWorkItemService workItemService,
        IState<FilterState> filterState,
        ISettingsRepository settingsRepository,
        ILogger<DashboardEffects> logger)
    {
        _repository = repository;
        _workItemService = workItemService;
        _filterState = filterState;
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    [EffectMethod]
    public async Task HandleLoadDashboard(LoadDashboardAction action, IDispatcher dispatcher)
    {
        try
        {
            var tiles = await _repository.GetPinnedTilesAsync();
            dispatcher.Dispatch(new DashboardLoadedAction(tiles));

            // Refresh work item data from AzDo in background
            if (tiles.Count > 0)
                _ = RefreshTilesFromAzDoAsync(tiles, dispatcher);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load dashboard");
            dispatcher.Dispatch(new DashboardLoadFailedAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandlePinWorkItem(PinWorkItemAction action, IDispatcher dispatcher)
    {
        try
        {
            var tile = new PinnedTile
            {
                WorkItemId = action.WorkItem.Id,
                Title = action.WorkItem.Title,
                State = action.WorkItem.State,
                WorkItemType = action.WorkItem.WorkItemType,
                ProjectName = action.WorkItem.ProjectName,
                TeamName = action.WorkItem.TeamName,
                IterationPath = action.WorkItem.IterationPath,
                CompletedWork = action.WorkItem.CompletedWork,
                RemainingWork = action.WorkItem.RemainingWork,
                PinnedAt = DateTimeOffset.UtcNow
            };

            await _repository.PinTileAsync(tile);
            dispatcher.Dispatch(new WorkItemPinnedAction(tile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to pin work item {Id}", action.WorkItem.Id);
        }
    }

    [EffectMethod]
    public async Task HandleUnpinWorkItem(UnpinWorkItemAction action, IDispatcher dispatcher)
    {
        try
        {
            await _repository.UnpinTileAsync(action.WorkItemId);
            dispatcher.Dispatch(new WorkItemUnpinnedAction(action.WorkItemId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unpin work item {Id}", action.WorkItemId);
        }
    }

    [EffectMethod]
    public async Task HandleRefreshTiles(RefreshTilesAction _, IDispatcher dispatcher)
    {
        try
        {
            var tiles = await _repository.GetPinnedTilesAsync();
            if (tiles.Count > 0)
                await RefreshTilesFromAzDoAsync(tiles, dispatcher);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to refresh tiles");
        }
    }

    private async Task RefreshTilesFromAzDoAsync(IReadOnlyList<PinnedTile> tiles, IDispatcher dispatcher)
    {
        try
        {
            var ids = tiles.Select(t => t.WorkItemId).ToArray();
            var workItems = await _workItemService.GetByIdsAsync(ids);
            var lookup = workItems.ToDictionary(w => w.Id);

            var updated = new List<PinnedTile>(tiles.Count);
            foreach (var tile in tiles)
            {
                if (lookup.TryGetValue(tile.WorkItemId, out var wi))
                {
                    var refreshed = tile with
                    {
                        Title = wi.Title,
                        State = wi.State,
                        CompletedWork = wi.CompletedWork,
                        RemainingWork = wi.RemainingWork,
                        IterationPath = wi.IterationPath
                    };
                    await _repository.UpdateTileAsync(refreshed);
                    updated.Add(refreshed);
                }
                else
                {
                    updated.Add(tile);
                }
            }

            dispatcher.Dispatch(new TilesRefreshedAction(updated));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Background tile refresh failed");
        }
    }

    [EffectMethod]
    public async Task HandlePinMySprintTasks(PinMySprintTasksAction _, IDispatcher dispatcher)
    {
        try
        {
            var filter = _filterState.Value;
            var project = filter.SelectedProject;
            var team = filter.SelectedTeam;

            if (string.IsNullOrEmpty(project) || string.IsNullOrEmpty(team))
            {
                var settings = await _settingsRepository.GetSettingsAsync();
                project ??= settings.DefaultProject;
                team ??= settings.DefaultTeam;
            }

            if (string.IsNullOrEmpty(project) || string.IsNullOrEmpty(team))
            {
                dispatcher.Dispatch(new PinMySprintTasksFailedAction("Select a project and team first."));
                return;
            }

            var tasks = await _workItemService.GetMyCurrentSprintTasksAsync(project, team);
            if (tasks.Count == 0)
            {
                dispatcher.Dispatch(new PinMySprintTasksSucceededAction([]));
                return;
            }

            var existingTiles = await _repository.GetPinnedTilesAsync();
            var existingIds = existingTiles.Select(t => t.WorkItemId).ToHashSet();

            var newTiles = new List<PinnedTile>();
            foreach (var wi in tasks)
            {
                if (existingIds.Contains(wi.Id))
                    continue;

                var tile = new PinnedTile
                {
                    WorkItemId = wi.Id,
                    Title = wi.Title,
                    State = wi.State,
                    WorkItemType = wi.WorkItemType,
                    ProjectName = wi.ProjectName,
                    TeamName = team,
                    IterationPath = wi.IterationPath,
                    CompletedWork = wi.CompletedWork,
                    RemainingWork = wi.RemainingWork,
                    PinnedAt = DateTimeOffset.UtcNow
                };

                await _repository.PinTileAsync(tile);
                newTiles.Add(tile);
            }

            dispatcher.Dispatch(new PinMySprintTasksSucceededAction(newTiles));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to pin sprint tasks");
            dispatcher.Dispatch(new PinMySprintTasksFailedAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleReorderTiles(ReorderTilesAction action, IDispatcher dispatcher)
    {
        try
        {
            await _repository.ReorderTilesAsync(action.WorkItemIdsInOrder);
            var tiles = await _repository.GetPinnedTilesAsync();
            dispatcher.Dispatch(new TilesReorderedAction(tiles));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reorder tiles");
        }
    }
}
