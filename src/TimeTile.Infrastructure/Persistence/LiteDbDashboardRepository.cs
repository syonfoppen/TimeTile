using LiteDB;
using TimeTile.Core.Interfaces;
using TimeTile.Core.Models;

namespace TimeTile.Infrastructure.Persistence;

public sealed class LiteDbDashboardRepository : IDashboardRepository
{
    private readonly ILiteCollection<PinnedTile> _tiles;

    public LiteDbDashboardRepository(DatabaseBootstrapper db)
    {
        _tiles = db.Database.GetCollection<PinnedTile>("pinned_tiles");
    }

    public Task<IReadOnlyList<PinnedTile>> GetPinnedTilesAsync(CancellationToken cancellationToken = default)
    {
        var tiles = _tiles.Query().OrderBy(t => t.SortOrder).ToList();
        return Task.FromResult<IReadOnlyList<PinnedTile>>(tiles);
    }

    public Task PinTileAsync(PinnedTile tile, CancellationToken cancellationToken = default)
    {
        var existing = _tiles.FindOne(t => t.WorkItemId == tile.WorkItemId);
        if (existing is not null)
            return Task.CompletedTask;

        var maxSort = _tiles.Query().OrderByDescending(t => t.SortOrder).FirstOrDefault()?.SortOrder ?? -1;
        var tileToInsert = tile with { SortOrder = maxSort + 1 };
        _tiles.Insert(tileToInsert);
        return Task.CompletedTask;
    }

    public Task UnpinTileAsync(int workItemId, CancellationToken cancellationToken = default)
    {
        _tiles.DeleteMany(t => t.WorkItemId == workItemId);
        return Task.CompletedTask;
    }

    public Task ReorderTilesAsync(IReadOnlyList<int> workItemIdsInOrder, CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < workItemIdsInOrder.Count; i++)
        {
            var id = workItemIdsInOrder[i];
            var tile = _tiles.FindOne(t => t.WorkItemId == id);
            if (tile is not null)
            {
                _tiles.Update(tile with { SortOrder = i });
            }
        }
        return Task.CompletedTask;
    }

    public Task UpdateTileAsync(PinnedTile tile, CancellationToken cancellationToken = default)
    {
        var existing = _tiles.FindOne(t => t.WorkItemId == tile.WorkItemId);
        if (existing is not null)
        {
            _tiles.Update(tile with { Id = existing.Id, SortOrder = existing.SortOrder });
        }
        return Task.CompletedTask;
    }
}
