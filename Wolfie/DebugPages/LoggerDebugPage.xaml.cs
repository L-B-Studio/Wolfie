
using System.Text;
using System.Text.Json;
using Wolfie.Auth;
using Wolfie.Helpers;
using Wolfie.Models;
using Wolfie.Services;

namespace Wolfie.DebugPages;

public partial class LoggerDebugPage : ContentPage
{
    private readonly StringBuilder _loggerString = new();
    private readonly SslClientService _client;
    private readonly AuthState _authstate;
    IDispatcherTimer _timer;

    public LoggerDebugPage()
    {
        InitializeComponent();
        _client = ServiceClientHelper.GetService<SslClientService>();
        _authstate = ServiceClientHelper.GetService<AuthState>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _client.MessageReceived += OnMessagedReceivedAsync;
        string token = _authstate?.AccessToken ?? "No Token";
        _loggerString.AppendLine($"[{DateTime.Now:HH:mm:ss}] Token: {token}");

        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(30);
        _timer.Tick += async (s, e) =>
        {
            _loggerString.Clear();
            try
            {
                _loggerString.AppendLine($"[{DateTime.Now:HH:mm:ss}] {token}");
                LoggerLabel.Text = _loggerString.ToString();
                await _client.SendJsonAsync(" get_logs_data", new Dictionary<string, string>
                {
                    ["token_access"] = token
                });
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Get loggs by access token error", ex.Message, "ok");
            }

        };

        // Запуск
        _timer.Start();

    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _client.MessageReceived -= OnMessagedReceivedAsync;
        if (_timer.IsRunning) _timer.Stop();
    }

    private async void OnMessagedReceivedAsync(string msg )
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {

            try
            {
                var packet = JsonSerializer.Deserialize<ServerJsonPackage>(msg);
                if (packet == null || string.IsNullOrWhiteSpace(packet.header)) return;
                if (packet.body == null) packet.body = new Dictionary<string, string>();
                switch (packet.header.Trim().ToLower())
                {
                    case "success":
                        _timer.Stop();
                        try
                        {
                            packet.body.TryGetValue("token_refresh", out string refresh_token);
                            refresh_token = refresh_token?.Trim();
                            await SecureStorage.SetAsync("refresh_token", refresh_token);
                            await DisplayAlertAsync("new refresh logger token", refresh_token, "ok");
                        }
                        catch (Exception ex)
                        {
                            await DisplayAlertAsync("Error in refresh token", ex.Message, "ok");
                        }
                        try
                        {
                            packet.body.TryGetValue("token_access", out string access_token);
                            access_token = access_token?.Trim();
                            _authstate.AccessToken = access_token;
                            _loggerString.AppendLine($"[{DateTime.Now:HH:mm:ss}] Token: {access_token}");
                            LoggerLabel.Text = _loggerString.ToString();
                            await DisplayAlertAsync("new access logger login token", access_token, "ok");
                        }
                        catch (Exception ex)
                        {
                            await DisplayAlertAsync("Error in access token", ex.Message, "ok");
                        }
                        _timer.Start();
                        return;
                    default:
                        return;
                }
            }
            catch (Exception ex) { }//await DisplayAlertAsync("get data error", ex.Message, "ok"); }

           
            _loggerString.AppendLine($"[{DateTime.Now:HH:mm:ss}] {msg}");
            LoggerLabel.Text = _loggerString.ToString();

            // Небольшая задержка, чтобы UI успел отрисовать новый текст
            await Task.Delay(50);

            // Скроллим к самому Label, позиционируя его конец (End) внизу ScrollView
            await LoggerScroll.ScrollToAsync(LoggerLabel, ScrollToPosition.End, true);
            
        });
    }

    async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    async void OnRefreshClicked(object sender, EventArgs e)
    {
        _loggerString.Clear();
        try
        {
            string? _refresh_token = await SecureStorage.GetAsync("refresh_token") ?? " no_token";

            await _client.SendJsonAsync("get_access_token_data", new Dictionary<string, string> {
                ["token_refresh"] = _refresh_token
            });
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Get refresh token error", ex.Message, "ok");
        }

    }

}