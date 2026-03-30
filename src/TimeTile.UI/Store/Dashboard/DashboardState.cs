using Fluxor;
using TimeTile.Core.Models;

namespace TimeTile.UI.Store.Dashboard;

[FeatureState]
public record DashboardState
{
    public IReadOnlyList<PinnedTile> Tiles { get; init; } = [];
    public bool IsLoading { get; init; }
    public bool IsPinningSprintTasks { get; init; }
    public string? ErrorMessage { get; init; }
}
