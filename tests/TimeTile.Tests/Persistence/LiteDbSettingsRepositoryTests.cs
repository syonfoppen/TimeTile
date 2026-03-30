using TimeTile.Core.Models;
using TimeTile.Infrastructure.Persistence;

namespace TimeTile.Tests.Persistence;

public class LiteDbSettingsRepositoryTests : IDisposable
{
    private readonly string _dbPath;
    private readonly DatabaseBootstrapper _bootstrapper;
    private readonly LiteDbSettingsRepository _repository;

    public LiteDbSettingsRepositoryTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"timetile_test_{Guid.NewGuid()}.db");
        _bootstrapper = new DatabaseBootstrapper(_dbPath);
        _repository = new LiteDbSettingsRepository(_bootstrapper);
    }

    [Fact]
    public async Task GetSettings_WhenNoSettingsSaved_ReturnsDefaults()
    {
        var settings = await _repository.GetSettingsAsync();

        Assert.Equal(string.Empty, settings.AzureDevOpsOrgUrl);
        Assert.Equal(string.Empty, settings.DefaultProject);
        Assert.Equal("common", settings.EntraTenantId);
    }

    [Fact]
    public async Task SaveSettings_ThenGetSettings_RoundTrips()
    {
        var settings = new AppSettings
        {
            AzureDevOpsOrgUrl = "https://dev.azure.com/myorg",
            SevenPaceOrgName = "myorg",
            SevenPaceApiBaseUrl = "https://api.7pace.com",
            DefaultProject = "MyProject",
            DefaultTeam = "MyTeam",
            DefaultIteration = "Sprint 1",
            EntraClientId = "client-id",
            EntraTenantId = "tenant-id"
        };

        await _repository.SaveSettingsAsync(settings);
        var loaded = await _repository.GetSettingsAsync();

        Assert.Equal(settings.AzureDevOpsOrgUrl, loaded.AzureDevOpsOrgUrl);
        Assert.Equal(settings.SevenPaceOrgName, loaded.SevenPaceOrgName);
        Assert.Equal(settings.SevenPaceApiBaseUrl, loaded.SevenPaceApiBaseUrl);
        Assert.Equal(settings.DefaultProject, loaded.DefaultProject);
        Assert.Equal(settings.DefaultTeam, loaded.DefaultTeam);
        Assert.Equal(settings.DefaultIteration, loaded.DefaultIteration);
        Assert.Equal(settings.EntraClientId, loaded.EntraClientId);
        Assert.Equal(settings.EntraTenantId, loaded.EntraTenantId);
    }

    [Fact]
    public async Task SaveSettings_CalledTwice_UpdatesExisting()
    {
        var first = new AppSettings { DefaultProject = "ProjectA" };
        var second = new AppSettings { DefaultProject = "ProjectB" };

        await _repository.SaveSettingsAsync(first);
        await _repository.SaveSettingsAsync(second);
        var loaded = await _repository.GetSettingsAsync();

        Assert.Equal("ProjectB", loaded.DefaultProject);
    }

    [Fact]
    public async Task GetOrganizations_WhenEmpty_ReturnsEmptyList()
    {
        var orgs = await _repository.GetOrganizationsAsync();

        Assert.Empty(orgs);
    }

    [Fact]
    public async Task SaveOrganization_ThenGet_RoundTrips()
    {
        var org = new OrgConfiguration
        {
            Id = "org-1",
            OrgUrl = "https://dev.azure.com/myorg",
            OrgName = "myorg",
            SevenPaceApiBaseUrl = "https://api.7pace.com",
            IsDefault = true
        };

        await _repository.SaveOrganizationAsync(org);
        var orgs = await _repository.GetOrganizationsAsync();

        Assert.Single(orgs);
        Assert.Equal("org-1", orgs[0].Id);
        Assert.Equal("myorg", orgs[0].OrgName);
        Assert.True(orgs[0].IsDefault);
    }

    [Fact]
    public async Task SaveOrganization_ExistingId_Updates()
    {
        var org = new OrgConfiguration { Id = "org-1", OrgName = "Original" };
        await _repository.SaveOrganizationAsync(org);

        var updated = org with { OrgName = "Updated" };
        await _repository.SaveOrganizationAsync(updated);

        var orgs = await _repository.GetOrganizationsAsync();
        Assert.Single(orgs);
        Assert.Equal("Updated", orgs[0].OrgName);
    }

    [Fact]
    public async Task DeleteOrganization_RemovesOrganization()
    {
        var org = new OrgConfiguration { Id = "org-1", OrgName = "TestOrg" };
        await _repository.SaveOrganizationAsync(org);

        await _repository.DeleteOrganizationAsync("org-1");
        var orgs = await _repository.GetOrganizationsAsync();

        Assert.Empty(orgs);
    }

    [Fact]
    public async Task DeleteOrganization_NonExistentId_DoesNotThrow()
    {
        await _repository.DeleteOrganizationAsync("non-existent");
    }

    public void Dispose()
    {
        _bootstrapper.Dispose();
        try { File.Delete(_dbPath); } catch { }
    }
}
