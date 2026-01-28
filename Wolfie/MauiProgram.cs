using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Wolfie.Auth;
using Wolfie.Helpers;
using Wolfie.Pages;
using Wolfie.Servers;
using Wolfie.Services;

namespace Wolfie
{
    public static class MauiProgram
    {
        //private static readonly SslClientService _client  = SslClientHelper.GetService<SslClientService>();
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
    .UseMauiApp<App>()
    .UseMauiCommunityToolkitMediaElement()  // ← Chain this here
    .ConfigureFonts(fonts =>
    {
        fonts.AddFont("PollerOne-Regular.ttf", "PollerOne");
        fonts.AddFont("MetalMania-Regular.ttf", "MetalMania");
        fonts.AddFont("PermanentMarker-Regular.ttf", "PermanentMarker");
        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        fonts.AddFont("GravitasOne-Regular.ttf", "GravitasOne");
        fonts.AddFont("Roboto-Regular.ttf", "Roboto");
    })
    .UseMauiCommunityToolkit();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<AuthState>();
            builder.Services.AddSingleton<HttpClientService>();
            builder.Services.AddSingleton<DeviceInfoHelper>();

            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<ChatListPage>();

            return builder.Build();
        }
    }
}

