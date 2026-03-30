namespace TimeTile.Core.Models;

public record AppSettings
{
    public string AzureDevOpsOrgUrl { get; init; } = string.Empty;
    public string SevenPaceOrgName { get; init; } = string.Empty;
    public string SevenPaceApiBaseUrl { get; init; } = string.Empty;
    public string DefaultProject { get; init; } = string.Empty;
    public string DefaultTeam { get; init; } = string.Empty;
    public string DefaultIteration { get; init; } = string.Empty;
    public string EntraClientId { get; init; } = string.Empty;
    public string EntraTenantId { get; init; } = "common";
}
