using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TimeTile.Core.Interfaces;
using TimeTile.Core.Models;

namespace TimeTile.Infrastructure.ApiClients.AzureDevOps;

public sealed class AzureDevOpsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAzureDevOpsAuthService _authService;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AzureDevOpsApiClient(HttpClient httpClient, IAzureDevOpsAuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    public async Task<IReadOnlyList<WorkItem>> QueryWorkItemsAsync(string wiql, CancellationToken cancellationToken = default)
    {
        await AttachTokenAsync(cancellationToken);

        var body = new { query = wiql };
        var response = await _httpClient.PostAsJsonAsync("_apis/wit/wiql?api-version=7.1", body, cancellationToken);
        response.EnsureSuccessStatusCode();

        var wiqlResult = await response.Content.ReadFromJsonAsync<WiqlResponse>(JsonOptions, cancellationToken);
        if (wiqlResult?.WorkItems is null || wiqlResult.WorkItems.Length == 0)
            return [];

        var ids = wiqlResult.WorkItems.Select(w => w.Id).Take(50).ToArray();
        return await GetWorkItemsByIdsAsync(ids, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkItem>> GetWorkItemsByIdsAsync(int[] ids, CancellationToken cancellationToken = default)
    {
        if (ids.Length == 0)
            return [];

        await AttachTokenAsync(cancellationToken);

        var idsParam = string.Join(",", ids);
        var fields = "System.Id,System.Title,System.State,System.WorkItemType,System.TeamProject,System.IterationPath,System.AreaPath,System.AssignedTo,Microsoft.VSTS.Scheduling.CompletedWork,Microsoft.VSTS.Scheduling.RemainingWork";
        var response = await _httpClient.GetAsync($"_apis/wit/workitems?ids={idsParam}&fields={fields}&api-version=7.1", cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<WorkItemsResponse>(JsonOptions, cancellationToken);
        if (result?.Value is null)
            return [];

        return result.Value.Select(w => new WorkItem
        {
            Id = w.Id,
            Title = w.Fields?.GetValueOrDefault("System.Title")?.ToString() ?? "",
            State = w.Fields?.GetValueOrDefault("System.State")?.ToString() ?? "",
            WorkItemType = w.Fields?.GetValueOrDefault("System.WorkItemType")?.ToString() ?? "",
            ProjectName = w.Fields?.GetValueOrDefault("System.TeamProject")?.ToString() ?? "",
            IterationPath = w.Fields?.GetValueOrDefault("System.IterationPath")?.ToString() ?? "",
            AreaPath = w.Fields?.GetValueOrDefault("System.AreaPath")?.ToString() ?? "",
            AssignedTo = ExtractDisplayName(w.Fields?.GetValueOrDefault("System.AssignedTo")),
            CompletedWork = ExtractDouble(w.Fields?.GetValueOrDefault("Microsoft.VSTS.Scheduling.CompletedWork")),
            RemainingWork = ExtractDouble(w.Fields?.GetValueOrDefault("Microsoft.VSTS.Scheduling.RemainingWork"))
        }).ToList();
    }

    public async Task<WorkItem?> GetWorkItemByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var items = await GetWorkItemsByIdsAsync([id], cancellationToken);
        return items.FirstOrDefault();
    }

    public async Task<IReadOnlyList<string>> GetProjectsAsync(CancellationToken cancellationToken = default)
    {
        await AttachTokenAsync(cancellationToken);

        var response = await _httpClient.GetAsync("_apis/projects?api-version=7.1", cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ProjectsResponse>(JsonOptions, cancellationToken);
        return result?.Value?.Select(p => p.Name).ToList() ?? [];
    }

    public async Task<IReadOnlyList<Team>> GetTeamsAsync(string project, CancellationToken cancellationToken = default)
    {
        await AttachTokenAsync(cancellationToken);

        var encodedProject = Uri.EscapeDataString(project);
        var response = await _httpClient.GetAsync($"_apis/projects/{encodedProject}/teams?api-version=7.1", cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TeamsResponse>(JsonOptions, cancellationToken);
        return result?.Value?.Select(t => new Team
        {
            Id = t.Id,
            Name = t.Name,
            ProjectName = project
        }).ToList() ?? [];
    }

    public async Task<IReadOnlyList<Sprint>> GetSprintsAsync(string project, string team, CancellationToken cancellationToken = default)
    {
        await AttachTokenAsync(cancellationToken);

        var encodedProject = Uri.EscapeDataString(project);
        var encodedTeam = Uri.EscapeDataString(team);
        var response = await _httpClient.GetAsync($"{encodedProject}/{encodedTeam}/_apis/work/teamsettings/iterations?api-version=7.1", cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<IterationsResponse>(JsonOptions, cancellationToken);
        return result?.Value?.Select(i => new Sprint
        {
            Id = i.Id,
            Name = i.Name,
            Path = i.Path,
            StartDate = i.Attributes?.StartDate,
            FinishDate = i.Attributes?.FinishDate
        }).ToList() ?? [];
    }

    private async Task AttachTokenAsync(CancellationToken cancellationToken)
    {
        var token = await _authService.GetAccessTokenAsync(cancellationToken);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static double? ExtractDouble(object? value)
    {
        if (value is JsonElement element && element.ValueKind == JsonValueKind.Number)
            return element.GetDouble();
        return null;
    }

    private static string ExtractDisplayName(object? value)
    {
        if (value is JsonElement element && element.ValueKind == JsonValueKind.Object)
        {
            return element.TryGetProperty("displayName", out var dn) ? dn.GetString() ?? "" : "";
        }
        return value?.ToString() ?? "";
    }

    // DTOs

    private sealed record WiqlResponse
    {
        [JsonPropertyName("workItems")] public WiqlWorkItem[]? WorkItems { get; init; }
    }

    private sealed record WiqlWorkItem
    {
        [JsonPropertyName("id")] public int Id { get; init; }
    }

    private sealed record WorkItemsResponse
    {
        [JsonPropertyName("value")] public WorkItemDto[]? Value { get; init; }
    }

    private sealed record WorkItemDto
    {
        [JsonPropertyName("id")] public int Id { get; init; }
        [JsonPropertyName("fields")] public Dictionary<string, object>? Fields { get; init; }
    }

    private sealed record ProjectsResponse
    {
        [JsonPropertyName("value")] public ProjectDto[]? Value { get; init; }
    }

    private sealed record ProjectDto
    {
        [JsonPropertyName("name")] public string Name { get; init; } = "";
    }

    private sealed record TeamsResponse
    {
        [JsonPropertyName("value")] public TeamDto[]? Value { get; init; }
    }

    private sealed record TeamDto
    {
        [JsonPropertyName("id")] public string Id { get; init; } = "";
        [JsonPropertyName("name")] public string Name { get; init; } = "";
    }

    private sealed record IterationsResponse
    {
        [JsonPropertyName("value")] public IterationDto[]? Value { get; init; }
    }

    private sealed record IterationDto
    {
        [JsonPropertyName("id")] public string Id { get; init; } = "";
        [JsonPropertyName("name")] public string Name { get; init; } = "";
        [JsonPropertyName("path")] public string Path { get; init; } = "";
        [JsonPropertyName("attributes")] public IterationAttributes? Attributes { get; init; }
    }

    private sealed record IterationAttributes
    {
        [JsonPropertyName("startDate")] public DateTimeOffset? StartDate { get; init; }
        [JsonPropertyName("finishDate")] public DateTimeOffset? FinishDate { get; init; }
    }
}
