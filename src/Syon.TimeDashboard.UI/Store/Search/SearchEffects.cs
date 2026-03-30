using Fluxor;
using Microsoft.Extensions.Logging;
using Syon.TimeDashboard.Core.Interfaces;

namespace Syon.TimeDashboard.UI.Store.Search;

public class SearchEffects
{
    private readonly IWorkItemService _workItemService;
    private readonly IState<Filter.FilterState> _filterState;
    private readonly ILogger<SearchEffects> _logger;

    public SearchEffects(IWorkItemService workItemService, IState<Filter.FilterState> filterState, ILogger<SearchEffects> logger)
    {
        _workItemService = workItemService;
        _filterState = filterState;
        _logger = logger;
    }

    [EffectMethod]
    public async Task HandleSearch(SearchWorkItemsAction action, IDispatcher dispatcher)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(action.Query))
            {
                dispatcher.Dispatch(new SearchResultsReceivedAction([]));
                return;
            }

            var filter = _filterState.Value;
            var results = await _workItemService.SearchAsync(
                action.Query,
                filter.SelectedProject,
                filter.SelectedTeam,
                filter.SelectedIteration);

            dispatcher.Dispatch(new SearchResultsReceivedAction(results));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search failed for query '{Query}'", action.Query);
            dispatcher.Dispatch(new SearchFailedAction(ex.Message));
        }
    }
}
