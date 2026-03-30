namespace Syon.TimeDashboard.Core.Models;

public record TrackingSession
{
    public int WorkItemId { get; init; }
    public string WorkItemTitle { get; init; } = string.Empty;
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? StoppedAt { get; init; }
    public TimeSpan Elapsed => (StoppedAt ?? DateTimeOffset.UtcNow) - StartedAt;
    public bool IsActive => StoppedAt is null;
}
