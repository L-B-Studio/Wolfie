using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Platform;
using Wolfie.Helpers;
using Wolfie.Pages;
using Wolfie.Servers;
using Wolfie.Services;

namespace Wolfie
{
    public partial class App : Application
    {
        private readonly HttpClientService _httpClient;// = new HttpClientService();
        public App(IServiceProvider services)
        {
            InitializeComponent();
            ThemeService.Init();

            _httpClient = services.GetRequiredService<HttpClientService>();
        }

        


        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());

#if WINDOWS
            window.Created += (s, e) =>
            {
                var mauiWindow = (Window)s;
                var nativeWindow = mauiWindow.Handler.PlatformView;

                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                var winId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(winId);

                // Задаём стартовый размер окна
                appWindow.Resize(new Windows.Graphics.SizeInt32(1280, 720));
            };
#endif

            return window;
        }
    }
}
