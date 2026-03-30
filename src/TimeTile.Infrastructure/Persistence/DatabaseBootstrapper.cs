using LiteDB;

namespace TimeTile.Infrastructure.Persistence;

public sealed class DatabaseBootstrapper : IDisposable
{
    private readonly LiteDatabase _database;

    public DatabaseBootstrapper(string databasePath)
    {
        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        _database = new LiteDatabase(databasePath);
        EnsureCollections();
    }

    public LiteDatabase Database => _database;

    private void EnsureCollections()
    {
        var tiles = _database.GetCollection("pinned_tiles");
        tiles.EnsureIndex("WorkItemId", unique: true);

        var settings = _database.GetCollection("settings");
        settings.EnsureIndex("Key", unique: true);

        var orgs = _database.GetCollection("organizations");
        orgs.EnsureIndex("Id", unique: true);
    }

    public void Dispose() => _database.Dispose();
}
