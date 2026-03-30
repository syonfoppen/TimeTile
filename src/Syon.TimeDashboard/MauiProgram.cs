using Fluxor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Syon.TimeDashboard.Core.Models;
using Syon.TimeDashboard.Infrastructure;

namespace Syon.TimeDashboard;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Configuration
        using var stream = typeof(MauiProgram).Assembly.GetManifestResourceStream("Syon.TimeDashboard.appsettings.json");
        if (stream is not null)
        {
            builder.Configuration.AddJsonStream(stream);
        }

        var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();

        // Blazor
        builder.Services.AddMauiBlazorWebView();

        // Fluxor state management
        builder.Services.AddFluxor(options =>
        {
            options.ScanAssemblies(typeof(Syon.TimeDashboard.UI.Store.Auth.AuthState).Assembly);
        });

        // Infrastructure (persistence, auth, API clients, services)
        builder.Services.AddInfrastructure(appSettings);

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
