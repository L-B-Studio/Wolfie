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
        _tcpService.MessageReceived -= OnMessageReceivedAsync;
        var packet = JsonSerializer.Deserialize<JsonPackage>(msg);
        switch (packet.header.ToLower().Trim())
        {
            case "error":
                packet.body.TryGetValue("error", out string error);
                error = error?.Trim().ToLower();
                if (error == "cant_send_mail")
                {
                    await ShowAlert("Error", "This email is unreal");
                }
                else if (error == "email_dont_register")
                {
                    await ShowAlert("Error", "This email is unregistered");
                }
                break;
            case "success":
                packet.body.TryGetValue("error", out string sucess);
                sucess = sucess?.Trim().ToLower();
                if (sucess == "mail_sended")
                {
                    await CloseAsync();
                    await Shell.Current.GoToAsync(nameof(EmailCodeVerifPopup));
                }
                break;
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

        await _tcpService.SendJsonAsync("forgotpass_data" , new() {["email"] = email});

    }

    private async Task ShowAlert(string title, string message)
    {
        await Application.Current.MainPage.DisplayAlertAsync(title, message, "OK");
    }
}

