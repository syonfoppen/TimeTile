namespace Syon.TimeDashboard.Core.Models;

public record Team
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;
}
