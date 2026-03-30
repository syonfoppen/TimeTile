namespace TimeTile.Core.Models;

public record WorkItem
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string WorkItemType { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;
    public string TeamName { get; init; } = string.Empty;
    public string IterationPath { get; init; } = string.Empty;
    public string AreaPath { get; init; } = string.Empty;
    public string AssignedTo { get; init; } = string.Empty;
    public double? CompletedWork { get; init; }
    public double? RemainingWork { get; init; }
}
