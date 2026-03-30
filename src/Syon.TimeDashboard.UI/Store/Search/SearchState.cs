using Fluxor;
using Syon.TimeDashboard.Core.Models;

namespace Syon.TimeDashboard.UI.Store.Search;

[FeatureState]
public record SearchState
{
    public string Query { get; init; } = string.Empty;
    public IReadOnlyList<WorkItem> Results { get; init; } = [];
    public bool IsSearching { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsOpen { get; init; }
}
