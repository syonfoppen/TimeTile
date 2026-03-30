using TimeTile.Core.Models;

namespace TimeTile.Core.Interfaces;

public interface ISettingsRepository
{
    Task<AppSettings> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task SaveSettingsAsync(AppSettings settings, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrgConfiguration>> GetOrganizationsAsync(CancellationToken cancellationToken = default);
    Task SaveOrganizationAsync(OrgConfiguration org, CancellationToken cancellationToken = default);
    Task DeleteOrganizationAsync(string id, CancellationToken cancellationToken = default);
}
