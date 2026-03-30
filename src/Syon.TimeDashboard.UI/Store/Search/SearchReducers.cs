using Fluxor;

namespace Syon.TimeDashboard.UI.Store.Search;

public static class SearchReducers
{
    [ReducerMethod]
    public static SearchState OnOpenSearch(SearchState state, OpenSearchAction _) =>
        state with { IsOpen = true };

    [ReducerMethod]
    public static SearchState OnCloseSearch(SearchState state, CloseSearchAction _) =>
        state with { IsOpen = false, Query = string.Empty, Results = [], ErrorMessage = null };

    [ReducerMethod]
    public static SearchState OnSearchWorkItems(SearchState state, SearchWorkItemsAction action) =>
        state with { Query = action.Query, IsSearching = true, ErrorMessage = null };

    [ReducerMethod]
    public static SearchState OnSearchResultsReceived(SearchState state, SearchResultsReceivedAction action) =>
        state with { Results = action.Results, IsSearching = false };

    [ReducerMethod]
    public static SearchState OnSearchFailed(SearchState state, SearchFailedAction action) =>
        state with { IsSearching = false, ErrorMessage = action.Error };

    [ReducerMethod]
    public static SearchState OnClearSearch(SearchState state, ClearSearchAction _) =>
        state with { Query = string.Empty, Results = [], ErrorMessage = null };
}
