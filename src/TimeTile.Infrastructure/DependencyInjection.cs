using Microsoft.Extensions.DependencyInjection;
using Polly;
using TimeTile.Core.Interfaces;
using TimeTile.Core.Models;
using TimeTile.Infrastructure.ApiClients.AzureDevOps;
using TimeTile.Infrastructure.ApiClients.SevenPace;
using TimeTile.Infrastructure.Persistence;
using TimeTile.Infrastructure.Security;
using TimeTile.Infrastructure.Services;

namespace TimeTile.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, AppSettings settings)
    {
        // Database
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TimeTile", "dashboard.db");

        services.AddSingleton(new DatabaseBootstrapper(dbPath));

        // Repositories
        services.AddSingleton<IDashboardRepository, LiteDbDashboardRepository>();
        services.AddSingleton<ISettingsRepository, LiteDbSettingsRepository>();

        // Secure token store (platform-specific)
        services.AddSingleton<ISecureTokenStore>(GetSecureTokenStore());

        // Azure DevOps Auth
        services.AddSingleton<IAzureDevOpsAuthService>(sp =>
            new AzureDevOpsAuthService(
                string.IsNullOrEmpty(settings.EntraClientId) ? "placeholder" : settings.EntraClientId,
                settings.EntraTenantId));

        // 7pace Auth
        services.AddSingleton<SevenPaceAuthService>();
        services.AddSingleton<ISevenPaceAuthService>(sp => sp.GetRequiredService<SevenPaceAuthService>());
        services.AddTransient<SevenPaceAuthHandler>();

        // 7pace HttpClient (for auth service — no auth handler, used for PIN/token endpoints)
        var sevenPaceBaseUrl = !string.IsNullOrEmpty(settings.SevenPaceApiBaseUrl)
            ? settings.SevenPaceApiBaseUrl.TrimEnd('/') + "/"
            : "https://softwareblocks.timehub.7pace.com/";

        services.AddHttpClient("SevenPaceAuth", client =>
        {
            client.BaseAddress = new Uri(sevenPaceBaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))));

        // 7pace HttpClient (for API calls — with auth handler, used for REST CRUD like workLogs)
        services.AddHttpClient<SevenPaceApiClient>(client =>
        {
            client.BaseAddress = new Uri(sevenPaceBaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddHttpMessageHandler<SevenPaceAuthHandler>()
        .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))));

        // Azure DevOps HttpClient
        var azDoBaseUrl = !string.IsNullOrEmpty(settings.AzureDevOpsOrgUrl)
            ? settings.AzureDevOpsOrgUrl.TrimEnd('/') + "/"
            : "https://dev.azure.com/placeholder/";

        services.AddHttpClient<AzureDevOpsApiClient>(client =>
        {
            client.BaseAddress = new Uri(azDoBaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))));

        // Services
        services.AddSingleton<ITimeTrackingService, TimeTrackingService>();
        services.AddSingleton<IWorkItemService, WorkItemService>();

        return services;
    }

    private static ISecureTokenStore GetSecureTokenStore()
    {
        if (OperatingSystem.IsWindows())
            return new WindowsSecureTokenStore();
        if (OperatingSystem.IsMacOS())
            return new MacOsSecureTokenStore();
        return new FallbackSecureTokenStore();
    }
}
