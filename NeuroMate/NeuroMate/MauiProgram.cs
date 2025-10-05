using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using NeuroMate.Services;
using CommunityToolkit.Maui;

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

            // Rejestracja serwisów
            builder.Services.AddSingleton<IFloatingAvatarService, FloatingAvatarService>();
            builder.Services.AddSingleton<INeuroScoreService, NeuroScoreService>();
            builder.Services.AddSingleton<IInterventionService, InterventionService>();
            builder.Services.AddSingleton<IPVTGameService, PvtGameService>();
            builder.Services.AddSingleton<IDataImportService, DataImportService>();
            
            // Nowe serwisy dla systemu punktów i awatarów
            builder.Services.AddSingleton<IPointsService, PointsService>();
            builder.Services.AddSingleton<IAvatarService, AvatarService>();
            builder.Services.AddSingleton<ILootBoxService, LootBoxService>();

            // Rejestracja stron
            builder.Services.AddTransient<Views.AvatarShopPage>();
            builder.Services.AddTransient<Views.LootBoxPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
