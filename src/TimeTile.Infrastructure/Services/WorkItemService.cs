using TimeTile.Core.Interfaces;
using TimeTile.Core.Models;
using TimeTile.Infrastructure.ApiClients.AzureDevOps;
using TimeTile.Infrastructure.ApiClients.SevenPace;

namespace TimeTile.Infrastructure.Services;

public sealed class WorkItemService : IWorkItemService
{
    private readonly SevenPaceApiClient _sevenPaceClient;
    private readonly AzureDevOpsApiClient _azDoClient;

    public WorkItemService(SevenPaceApiClient sevenPaceClient, AzureDevOpsApiClient azDoClient)
    {
        _sevenPaceClient = sevenPaceClient;
        _azDoClient = azDoClient;
    }

    public async Task<IReadOnlyList<WorkItem>> SearchAsync(string query, string? project = null, string? team = null, string? iteration = null, CancellationToken cancellationToken = default)
    {
        // Try 7pace search first
        try
        {
            var results = await _sevenPaceClient.SearchWorkItemsAsync(query, cancellationToken);
            if (results.Count > 0)
                return results;
        }
        catch
        {
            // Fall through to AzDO WIQL
        }

        // Fallback to AzDO WIQL
        return await SearchViaWiqlAsync(query, project, team, iteration, cancellationToken);
    }

    public async Task<WorkItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _azDoClient.GetWorkItemByIdAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkItem>> GetByIdsAsync(int[] ids, CancellationToken cancellationToken = default)
    {
        return await _azDoClient.GetWorkItemsByIdsAsync(ids, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkItem>> GetMyCurrentSprintTasksAsync(string project, string team, CancellationToken cancellationToken = default)
    {
        var sprints = await _azDoClient.GetSprintsAsync(project, team, cancellationToken);
        var currentSprint = sprints.FirstOrDefault(s => s.IsCurrent);
        if (currentSprint is null)
            return [];

        var wiql = $"SELECT [System.Id] FROM WorkItems WHERE [System.WorkItemType] = 'Task' AND [System.AssignedTo] = @Me AND [System.IterationPath] = '{EscapeWiql(currentSprint.Path)}' AND [System.State] <> 'Closed' AND [System.State] <> 'Removed' ORDER BY [System.State] ASC";
        return await _azDoClient.QueryWorkItemsAsync(wiql, cancellationToken);
    }

    public async Task<IReadOnlyList<Team>> GetTeamsAsync(string project, CancellationToken cancellationToken = default)
    {
        return await _azDoClient.GetTeamsAsync(project, cancellationToken);
    }

    public async Task<IReadOnlyList<Sprint>> GetSprintsAsync(string project, string team, CancellationToken cancellationToken = default)
    {
        return await _azDoClient.GetSprintsAsync(project, team, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetProjectsAsync(CancellationToken cancellationToken = default)
    {
        return await _azDoClient.GetProjectsAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<WorkItem>> SearchViaWiqlAsync(string query, string? project, string? team, string? iteration, CancellationToken cancellationToken)
    {
        var conditions = new List<string>();

        // Check if query is a numeric ID
        if (int.TryParse(query, out var workItemId))
        {
            conditions.Add($"[System.Id] = {workItemId}");
        }
        else
        {
            conditions.Add($"[System.Title] CONTAINS '{EscapeWiql(query)}'");
        }

        if (!string.IsNullOrEmpty(project))
            conditions.Add($"[System.TeamProject] = '{EscapeWiql(project)}'");

        if (!string.IsNullOrEmpty(iteration))
            conditions.Add($"[System.IterationPath] UNDER '{EscapeWiql(iteration)}'");

        var whereClause = string.Join(" AND ", conditions);
        var wiql = $"SELECT [System.Id] FROM WorkItems WHERE {whereClause} ORDER BY [System.ChangedDate] DESC";

        return await _azDoClient.QueryWorkItemsAsync(wiql, cancellationToken);
    }

    private static string EscapeWiql(string value) => value.Replace("'", "''");
}
