namespace TimeTile.Core.Models;

public record PinnedTile
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public int WorkItemId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string WorkItemType { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;
    public string TeamName { get; init; } = string.Empty;
    public string IterationPath { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public DateTimeOffset PinnedAt { get; init; } = DateTimeOffset.UtcNow;
    public double? CompletedWork { get; init; }
    public double? RemainingWork { get; init; }
}
