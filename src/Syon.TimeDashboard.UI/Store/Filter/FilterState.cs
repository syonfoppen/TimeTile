using Fluxor;
using Syon.TimeDashboard.Core.Models;

namespace Syon.TimeDashboard.UI.Store.Filter;

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
