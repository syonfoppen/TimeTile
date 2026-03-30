using Syon.TimeDashboard.Core.Models;

namespace Syon.TimeDashboard.Core.Interfaces;

public interface IDashboardRepository
{
    Task<IReadOnlyList<PinnedTile>> GetPinnedTilesAsync(CancellationToken cancellationToken = default);
    Task PinTileAsync(PinnedTile tile, CancellationToken cancellationToken = default);
    Task UnpinTileAsync(int workItemId, CancellationToken cancellationToken = default);
    Task ReorderTilesAsync(IReadOnlyList<int> workItemIdsInOrder, CancellationToken cancellationToken = default);
    Task UpdateTileAsync(PinnedTile tile, CancellationToken cancellationToken = default);
}
