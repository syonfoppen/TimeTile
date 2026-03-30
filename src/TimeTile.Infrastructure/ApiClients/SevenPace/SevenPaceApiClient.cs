using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using TimeTile.Core.Models;

namespace TimeTile.Infrastructure.ApiClients.SevenPace;

public sealed class SevenPaceApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SevenPaceApiClient> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public SevenPaceApiClient(HttpClient httpClient, ILogger<SevenPaceApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<TrackingSession?> GetCurrentTrackingAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/tracking/client/current?api-version=3.2&expand=true", cancellationToken);
        _logger.LogInformation("7pace GET current -> {StatusCode}", (int)response.StatusCode);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("7pace GET current error body: {Body}", body);
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<TrackingStateResponse>(JsonOptions, cancellationToken);
        return MapTrackingState(result);
    }

    public async Task<TrackingSession> StartTrackingAsync(int workItemId, CancellationToken cancellationToken = default)
    {
        var body = new
        {
            timeZone = (int)TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes,
            tfsId = workItemId,
            remark = (string?)null,
            activityTypeId = (string?)null,
            isBillable = (bool?)null
        };
        var response = await _httpClient.PostAsJsonAsync("api/tracking/client/startTracking?api-version=3.2", body, JsonOptions, cancellationToken);
        _logger.LogInformation("7pace POST startTracking -> {StatusCode}", (int)response.StatusCode);
        if (!response.IsSuccessStatusCode)
        {
            var errBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("7pace POST startTracking error body: {Body}", errBody);
        }
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TrackingStateResponse>(JsonOptions, cancellationToken);
        return MapTrackingState(result) ?? new TrackingSession
        {
            WorkItemId = workItemId,
            WorkItemTitle = $"Work Item {workItemId}",
            StartedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>Stop tracking. reason: 0 = stopped by client, 1 = stopped by lock/idle.</summary>
    public async Task StopTrackingAsync(int reason = 0, CancellationToken cancellationToken = default)
    {
        // OpenAPI spec: POST /api/tracking/client/stopTracking/{reason} where reason is integer
        var response = await _httpClient.PostAsync($"api/tracking/client/stopTracking/{reason}?api-version=3.2", null, cancellationToken);
        _logger.LogInformation("7pace POST stopTracking/{Reason} -> {StatusCode}", reason, (int)response.StatusCode);
        if (!response.IsSuccessStatusCode)
        {
            var errBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("7pace POST stopTracking error body: {Body}", errBody);
        }
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<TrackingSession>> GetLatestTracksAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/tracking/client/latest/{count}?api-version=3.2", cancellationToken);
        _logger.LogInformation("7pace GET latest -> {StatusCode}", (int)response.StatusCode);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("7pace GET latest error body: {Body}", body);
            return [];
        }

        var result = await response.Content.ReadFromJsonAsync<LatestWorkLogsResponse>(JsonOptions, cancellationToken);
        if (result?.Data?.WorkLogs is null)
            return [];

        return result.Data.WorkLogs
            .Select(MapWorkLogToSession)
            .ToList();
    }

    public async Task<IReadOnlyList<WorkItem>> SearchWorkItemsAsync(string query, CancellationToken cancellationToken = default)
    {
        // API expects the body to be a plain JSON string value, not an object
        var response = await _httpClient.PostAsJsonAsync("api/tracking/client/search?api-version=3.2", query, cancellationToken);
        _logger.LogInformation("7pace POST search -> {StatusCode}", (int)response.StatusCode);
        if (!response.IsSuccessStatusCode)
        {
            var errBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("7pace POST search error body: {Body}", errBody);
            return [];
        }

        var result = await response.Content.ReadFromJsonAsync<SearchResultResponse>(JsonOptions, cancellationToken);
        if (result?.Data?.WorkItems is null)
            return [];

        return result.Data.WorkItems.Select(entry =>
        {
            var wi = entry.WorkItem;
            return new WorkItem
            {
                Id = wi?.Id ?? 0,
                Title = wi?.Title ?? "",
                State = "",
                WorkItemType = wi?.Type ?? "",
                ProjectName = wi?.TeamProject ?? "",
                IterationPath = ""
            };
        }).ToList();
    }

    private static TrackingSession? MapTrackingState(TrackingStateResponse? response)
    {
        var track = response?.Data?.Track;
        if (track is null)
            return null;

        var trackingState = track.TrackingState ?? "";
        var isIdle = trackingState is "idle" or "";

        var workItemId = track.TfsId ?? 0;
        string? title = track.WorkItem?.Title;
        if (workItemId == 0 && track.WorkItem?.Id is > 0)
            workItemId = track.WorkItem.Id.Value;
        if (string.IsNullOrEmpty(title))
            title = track.Remark;

        if (isIdle && track.CurrentTrackStartedDateTime == null && workItemId == 0)
            return null;

        return new TrackingSession
        {
            WorkItemId = workItemId,
            WorkItemTitle = title ?? (workItemId > 0 ? $"Work Item {workItemId}" : "Non-DevOps tracking"),
            StartedAt = track.CurrentTrackStartedDateTime ?? DateTimeOffset.UtcNow,
            StoppedAt = isIdle ? DateTimeOffset.UtcNow : null
        };
    }

    private static TrackingSession MapWorkLogToSession(WorkLogEntry entry) => new()
    {
        WorkItemId = entry.WorkItem?.Id ?? 0,
        WorkItemTitle = entry.WorkItem?.Title ?? (entry.WorkItem?.Id > 0 ? $"Work Item {entry.WorkItem.Id}" : "Non-DevOps tracking"),
        StartedAt = entry.StartTime ?? DateTimeOffset.UtcNow,
        StoppedAt = entry.EndTime
    };

    #region DTOs matching 7pace OpenAPI v3.2 schemas

    // trackingStateModel: { data: { track, trackSettings, settings, timestamp } }
    private sealed record TrackingStateResponse
    {
        [JsonPropertyName("data")] public TrackingStateData? Data { get; init; }
    }

    private sealed record TrackingStateData
    {
        [JsonPropertyName("track")] public TrackData? Track { get; init; }
    }

    private sealed record TrackData
    {
        [JsonPropertyName("tfsId")] public int? TfsId { get; init; }
        [JsonPropertyName("remark")] public string? Remark { get; init; }
        [JsonPropertyName("trackingState")] public string? TrackingState { get; init; }
        [JsonPropertyName("currentTrackStartedDateTime")] public DateTimeOffset? CurrentTrackStartedDateTime { get; init; }
        [JsonPropertyName("currentTrackLength")] public double? CurrentTrackLength { get; init; }
        [JsonPropertyName("workItem")] public WorkItemData? WorkItem { get; init; }
    }

    private sealed record WorkItemData
    {
        [JsonPropertyName("id")] public int? Id { get; init; }
        [JsonPropertyName("title")] public string? Title { get; init; }
        [JsonPropertyName("color")] public string? Color { get; init; }
        [JsonPropertyName("teamProject")] public string? TeamProject { get; init; }
        [JsonPropertyName("type")] public string? Type { get; init; }
    }

    // latestWorkLogsModel: { data: { count, workLogs: [...] } }
    private sealed record LatestWorkLogsResponse
    {
        [JsonPropertyName("data")] public LatestWorkLogsData? Data { get; init; }
    }

    private sealed record LatestWorkLogsData
    {
        [JsonPropertyName("count")] public int? Count { get; init; }
        [JsonPropertyName("workLogs")] public WorkLogEntry[]? WorkLogs { get; init; }
    }

    private sealed record WorkLogEntry
    {
        [JsonPropertyName("id")] public string? Id { get; init; }
        [JsonPropertyName("startTime")] public DateTimeOffset? StartTime { get; init; }
        [JsonPropertyName("endTime")] public DateTimeOffset? EndTime { get; init; }
        [JsonPropertyName("periodLength")] public double? PeriodLength { get; init; }
        [JsonPropertyName("remark")] public string? Remark { get; init; }
        [JsonPropertyName("workItem")] public WorkItemData? WorkItem { get; init; }
    }

    // searchResultModel: { data: { query, workItems: [{ groupName, workItem }], count } }
    private sealed record SearchResultResponse
    {
        [JsonPropertyName("data")] public SearchResultData? Data { get; init; }
    }

    private sealed record SearchResultData
    {
        [JsonPropertyName("query")] public string? Query { get; init; }
        [JsonPropertyName("count")] public int? Count { get; init; }
        [JsonPropertyName("workItems")] public WorkItemSearchEntry[]? WorkItems { get; init; }
    }

    private sealed record WorkItemSearchEntry
    {
        [JsonPropertyName("groupName")] public string? GroupName { get; init; }
        [JsonPropertyName("workItem")] public WorkItemData? WorkItem { get; init; }
    }

    #endregion
}
