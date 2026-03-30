using Fluxor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TimeTile.Core.Models;
using TimeTile.Infrastructure;

namespace TimeTile;

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
        using var stream = typeof(MauiProgram).Assembly.GetManifestResourceStream("TimeTile.appsettings.json");
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
            options.ScanAssemblies(typeof(TimeTile.UI.Store.Auth.AuthState).Assembly);
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
