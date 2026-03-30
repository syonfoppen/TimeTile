using Syon.TimeDashboard.Core.Models;

namespace Syon.TimeDashboard.Core.Interfaces;

public interface IWorkItemService
{
    Task<IReadOnlyList<WorkItem>> SearchAsync(string query, string? project = null, string? team = null, string? iteration = null, CancellationToken cancellationToken = default);
    Task<WorkItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkItem>> GetByIdsAsync(int[] ids, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkItem>> GetMyCurrentSprintTasksAsync(string project, string team, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Team>> GetTeamsAsync(string project, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Sprint>> GetSprintsAsync(string project, string team, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetProjectsAsync(CancellationToken cancellationToken = default);
}
