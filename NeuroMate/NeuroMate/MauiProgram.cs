using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using NeuroMate.Services;
using CommunityToolkit.Maui;
using NeuroMate.Database;

namespace NeuroMate
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Rejestracja bazy danych
            builder.Services.AddSingleton<DatabaseService>();

            // Rejestracja serwisów
            builder.Services.AddSingleton<IFloatingAvatarService, FloatingAvatarService>();
            builder.Services.AddSingleton<INeuroScoreService, NeuroScoreService>();
            builder.Services.AddSingleton<IInterventionService, InterventionService>();
            builder.Services.AddSingleton<IPVTGameService, PvtGameService>();
            builder.Services.AddSingleton<IDataImportService, DataImportService>();
            
            // Nowe serwisy dla systemu punktów i lootboxów
            builder.Services.AddSingleton<IPointsService, PointsService>();
            builder.Services.AddSingleton<PointsService>();
            builder.Services.AddSingleton<IAvatarService, AvatarService>();
            builder.Services.AddSingleton<AvatarService>();
            builder.Services.AddSingleton<LootBoxService>();

            // Rejestracja stron
            builder.Services.AddTransient<Views.AvatarShopPage>();
            builder.Services.AddTransient<Views.LootBoxPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            var app = builder.Build();
            App.Services = app.Services;
            return app;
        }
    }
}
