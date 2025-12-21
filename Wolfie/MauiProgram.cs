using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Wolfie.Auth;
using Wolfie.Helpers;
using Wolfie.Pages;
using Wolfie.Services;
using Wolfie.ViewModels;
using static Wolfie.ViewModels.ChatItemViewModel;

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
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("PollerOne-Regular.ttf", "PollerOne");
                    fonts.AddFont("MetalMania-Regular.ttf", "MetalMania");
                    fonts.AddFont("PermanentMarker-Regular.ttf", "PermanentMarker");
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                }).UseMauiCommunityToolkit().UseMauiCommunityToolkitMediaElement();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<SslClientService>();
            builder.Services.AddSingleton<ChatStorageService>();
            builder.Services.AddSingleton<LocalDbService>();
            builder.Services.AddSingleton<AuthState>();

            builder.Services.AddTransient<ChatItemViewModel>();
            builder.Services.AddTransient<ChatListPage>();
            builder.Services.AddSingleton<AuthState>();

            return builder.Build();
        }
    }
}
