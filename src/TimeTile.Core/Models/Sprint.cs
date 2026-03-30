namespace TimeTile.Core.Models;

public record Sprint
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? FinishDate { get; init; }
    public bool IsCurrent => StartDate <= DateTimeOffset.UtcNow && FinishDate >= DateTimeOffset.UtcNow;
}
