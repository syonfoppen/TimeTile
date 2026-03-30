using Fluxor;
using TimeTile.Core.Models;

namespace TimeTile.UI.Store.Filter;

[FeatureState]
public record FilterState
{
    public IReadOnlyList<string> Projects { get; init; } = [];
    public IReadOnlyList<Team> Teams { get; init; } = [];
    public IReadOnlyList<Sprint> Sprints { get; init; } = [];
    public string? SelectedProject { get; init; }
    public string? SelectedTeam { get; init; }
    public string? SelectedIteration { get; init; }
    public bool IsLoadingFilters { get; init; }
}
