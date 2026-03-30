using TimeTile.Core.Models;

namespace TimeTile.UI.Store.Search;

public record OpenSearchAction;
public record CloseSearchAction;

public record SearchWorkItemsAction(string Query);
public record SearchResultsReceivedAction(IReadOnlyList<WorkItem> Results);
public record SearchFailedAction(string Error);
public record ClearSearchAction;
