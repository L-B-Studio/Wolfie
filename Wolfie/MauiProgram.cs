using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Wolfie.Helpers;
using Wolfie.Services;
using Wolfie.ViewModels;

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
            builder.Services.AddTransient<ChatPageViewModel>();
            //_ = StartConnection();

            return builder.Build();
        }

        //private static async Task StartConnection()
        //{
        //    try
        //    {
        //        await _client.EnsureConnectedAsync();
        //    }
        //    catch (Exception)
        //    {
        //        _ = StartConnection();
        //    }
        //}
    }
}
