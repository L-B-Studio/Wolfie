using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Wolfie.Services;

namespace Wolfie
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("PollerOne-Regular.ttf", "PollerOne");
                    fonts.AddFont("MetalMania-Regular.ttf", "MetalMania");
                    fonts.AddFont("PermanentMarker-Regular.tёtf", "PermanentMarker");
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                }).UseMauiCommunityToolkit();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<SslClientService>();
            builder.Services.AddSingleton<ChatStorageService>();

            return builder.Build();
        }
    }
}
