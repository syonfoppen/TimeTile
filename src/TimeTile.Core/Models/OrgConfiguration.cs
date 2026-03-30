namespace TimeTile.Core.Models;

public record OrgConfiguration
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string OrgUrl { get; init; } = string.Empty;
    public string OrgName { get; init; } = string.Empty;
    public string SevenPaceApiBaseUrl { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
}
