using CommunityToolkit.Maui.Views;
using System.Text.Json;
using Wolfie.Helpers;
using Wolfie.Models;
using Wolfie.Pages;
using Wolfie.Services;

namespace Wolfie.Popups;

public partial class EmailCodeVerifPopup : Popup
{
    private readonly SslClientService _tcpService;

    public EmailCodeVerifPopup()
    {
        InitializeComponent();
        _tcpService = SslClientHelper.GetService<SslClientService>();
        _tcpService.MessageReceived += OnMessageReceived;
    }

    private async void OnMessageReceived(string msg)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            _tcpService.MessageReceived -= OnMessageReceived;

            var packet = JsonSerializer.Deserialize<GetJsonPackage>(msg);
            switch (packet.status.ToLower().Trim())
            {
                case "error":
                    if (packet.data.GetProperty("error").GetString().ToLower().Trim() == "cant_send_email")
                    {
                        await Application.Current.MainPage
                            .DisplayAlertAsync("Error", "This email is unreal" , "ok");
                    }
                    if (packet.data.GetProperty("error").GetString().ToLower().Trim() == "invalid_code")
                    {
                        await Application.Current.MainPage
                            .DisplayAlertAsync("Error", "This email is unreal", "ok");
                    }
                    break;

                case "success":
                    if (packet.data.GetProperty("message").GetString().ToLower().Trim() == "email_verified")
                    {
                        await Application.Current.MainPage
                            .DisplayAlertAsync("Успех", "Почта подтверждена", "OK");
                        // закрываем попап
                        await CloseAsync();
                        // переходим на главную
                        await Shell.Current.GoToAsync(nameof(ChangedPasswordPopup));
                    }
                    break;
                default :
                    break;
            }
        });
    }



    private async void VerifButtonClicked(object sender, EventArgs e)
    {
        string verifCode = CodeEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(verifCode) || verifCode.Length < 9)
        {
            await Application.Current.MainPage.DisplayAlertAsync(
               "Ошибка", "Введите корректный код", "OK");
            return;
        }

        try
        {
            //string message = $"verify_data;{verifCode}";
            await _tcpService.SendJsonAsync("verify_data" , new {code = verifCode});
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlertAsync("Ошибка", ex.Message, "OK");
        }
    }
}