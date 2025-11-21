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
        try
        {
            var packet = JsonSerializer.Deserialize<JsonPackage>(msg);
            if (packet == null || string.IsNullOrWhiteSpace(packet.header)) return;
            if (packet.body == null) packet.body = new Dictionary<string, string>();

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                switch (packet.header.Trim().ToLower())
                {
                    case "error":
                        packet.body.TryGetValue("error", out string error);
                        error = error?.Trim().ToLower();
                        if (error == "cant_send_mail")
                            await ShowAlert("Error", "This email is unreal");
                        else if (error == "email_dont_register")
                            await ShowAlert("Error", "This email is unregistered");
                        break;

                    case "success":
                        packet.body.TryGetValue("success", out string success);
                        success = success?.Trim().ToLower();
                        if (success == "mail_sended")
                        {
                            var newPopup = new EmailCodeVerifPopup();
                            await Task.Delay(100); // чтобы UI успел обновиться
                            await CloseAsync();
                            await Application.Current.MainPage.ShowPopupAsync(newPopup);
                            // НЕ закрываем текущий попап сразу
                        }
                        break;
                }
            });
        }
        catch (Exception ex)
        {
            // логируем исключение, не крашим приложение
            Console.WriteLine($"MessageReceived error: {ex}");

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Application.Current.MainPage.DisplayAlertAsync("Ошибка", ex.Message, "OK");
            });
        }
    }





    private async void SendCodeButtonClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim();
        string dataMessage = $"forgotpass_data ; {email}";

        if (string.IsNullOrEmpty(email))
        {
            await ShowAlert("Ошибка", "Все поля должны быть заполнены");
            return;
        }
        try
        {
            await _tcpService.SendJsonAsync("forgotpass_data", new() { ["email"] = email });
            await Task.Delay(500);
        }
        catch (Exception ex) { 
            await ShowAlert("Error: ", ex.Message);
        }

    }

    private async Task ShowAlert(string title, string message)  
    {
        await Application.Current.MainPage.DisplayAlertAsync(title, message, "OK");
    }
}

