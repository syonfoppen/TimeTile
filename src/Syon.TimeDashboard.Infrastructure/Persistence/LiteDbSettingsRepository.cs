using LiteDB;
using Syon.TimeDashboard.Core.Interfaces;
using Syon.TimeDashboard.Core.Models;

namespace Syon.TimeDashboard.Infrastructure.Persistence;

public sealed class LiteDbSettingsRepository : ISettingsRepository
{
    private readonly ILiteCollection<BsonDocument> _settings;
    private readonly ILiteCollection<OrgConfiguration> _orgs;
    private const string SettingsKey = "app_settings";

    public LiteDbSettingsRepository(DatabaseBootstrapper db)
    {
        _settings = db.Database.GetCollection("settings");
        _orgs = db.Database.GetCollection<OrgConfiguration>("organizations");
    }

    public Task<AppSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var doc = _settings.FindOne(d => d["Key"] == SettingsKey);
        if (doc is null)
            return Task.FromResult(new AppSettings());

        var settings = new AppSettings
        {
            AzureDevOpsOrgUrl = doc["AzureDevOpsOrgUrl"]?.AsString ?? "",
            SevenPaceOrgName = doc["SevenPaceOrgName"]?.AsString ?? "",
            SevenPaceApiBaseUrl = doc["SevenPaceApiBaseUrl"]?.AsString ?? "",
            DefaultProject = doc["DefaultProject"]?.AsString ?? "",
            DefaultTeam = doc["DefaultTeam"]?.AsString ?? "",
            DefaultIteration = doc["DefaultIteration"]?.AsString ?? "",
            EntraClientId = doc["EntraClientId"]?.AsString ?? "",
            EntraTenantId = doc["EntraTenantId"]?.AsString ?? "common"
        };
        return Task.FromResult(settings);
    }

    public Task SaveSettingsAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        var doc = new BsonDocument
        {
            ["Key"] = SettingsKey,
            ["AzureDevOpsOrgUrl"] = settings.AzureDevOpsOrgUrl,
            ["SevenPaceOrgName"] = settings.SevenPaceOrgName,
            ["SevenPaceApiBaseUrl"] = settings.SevenPaceApiBaseUrl,
            ["DefaultProject"] = settings.DefaultProject,
            ["DefaultTeam"] = settings.DefaultTeam,
            ["DefaultIteration"] = settings.DefaultIteration,
            ["EntraClientId"] = settings.EntraClientId,
            ["EntraTenantId"] = settings.EntraTenantId
        };

        var existing = _settings.FindOne(d => d["Key"] == SettingsKey);
        if (existing is not null)
        {
            doc["_id"] = existing["_id"];
            _settings.Update(doc);
        }
        else
        {
            _settings.Insert(doc);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<OrgConfiguration>> GetOrganizationsAsync(CancellationToken cancellationToken = default)
    {
        var orgs = _orgs.FindAll().ToList();
        return Task.FromResult<IReadOnlyList<OrgConfiguration>>(orgs);
    }

    public Task SaveOrganizationAsync(OrgConfiguration org, CancellationToken cancellationToken = default)
    {
        var existing = _orgs.FindOne(o => o.Id == org.Id);
        if (existing is not null)
            _orgs.Update(org);
        else
            _orgs.Insert(org);
        return Task.CompletedTask;
    }

    public Task DeleteOrganizationAsync(string id, CancellationToken cancellationToken = default)
    {
        _orgs.DeleteMany(o => o.Id == id);
        return Task.CompletedTask;
    }
}
