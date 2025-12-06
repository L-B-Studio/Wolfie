using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using System.Text.Json;
using Wolfie.Helpers;
using Wolfie.Models;
using Wolfie.Pages;
using Wolfie.Services;

namespace Wolfie.Popups;

public partial class ForgotPassPopup : Popup
{
    private readonly SslClientService _tcpService;
    public ForgotPassPopup()
    {
        InitializeComponent();
        _tcpService = SslClientHelper.GetService<SslClientService>();
        _tcpService.MessageReceived += OnMessageReceivedAsync;
    }

    private async void OnMessageReceivedAsync(string msg)
    {

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            string email = EmailEntry.Text?.Trim();
            try
            {
                try
                {
                    JsonDocument.Parse(msg);
                }
                catch (Exception ex)
                {
                    await ShowAlert("Error", ex.Message);
                    return;
                }
                

                var packet = JsonSerializer.Deserialize<JsonPackage>(msg);
                if (packet == null || string.IsNullOrWhiteSpace(packet.header)) return;
                if (packet.body == null) packet.body = new Dictionary<string, string>();

                switch (packet.header.Trim().ToLower())
                {
                    case "error": 
                        packet.body.TryGetValue("error", out string error);
                        error = error?.Trim().ToLower();
                        if (error == "forgotpass_failed;email_not_found")
                            await ShowAlert("Error", $"{msg}\nThis email is unregistered");
                        return;

                    case "success": 
                        packet.body.TryGetValue("success", out string success);
                        success = success?.Trim().ToLower();
                        if (success == "forgotpass_success;confirmation_code_sent")
                        {
                            var newPopup = new EmailCodeVerifPopup(email);
                            await Task.Delay(100); // чтобы UI успел обновиться
                            await CloseAsync();
                            await Application.Current.MainPage.ShowPopupAsync(newPopup);
                            // НЕ закрываем текущий попап сразу
                        }
                        break;
                    default: return;
                }

            }
            catch (Exception ex)
            {
                // логируем исключение, не крашим приложение
                Console.WriteLine($"MessageReceived error: {ex}");

                await ShowAlert("Ошибка", ex.Message);
                return;
            }
        });
    }


    private async void SendCodeButtonClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim();
        string dataMessage = $"forgotpass_data ; {email}";

        if (string.IsNullOrEmpty(email))
        {
            await ShowAlert("Error", "All rows must be filled");
            return;
        }
        try
        {
            await _tcpService.SendJsonAsync("forgotpass_data", new() { ["email"] = email });
            await Task.Delay(500);
        }
        catch (Exception ex)
        {
            await ShowAlert("Error: ", ex.Message);
        }

    }

    private async Task ShowAlert(string title, string message)
    {
        await Application.Current.MainPage.DisplayAlertAsync(title, message, "OK");
    }
}

