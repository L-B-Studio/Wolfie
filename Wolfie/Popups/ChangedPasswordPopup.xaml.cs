using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using System.Text.Json;
using Wolfie.Helpers;
using Wolfie.Models;
using Wolfie.Services;

namespace Wolfie.Popups;

public partial class ChangedPasswordPopup : Popup
{
    private readonly SslClientService _client;
    public ChangedPasswordPopup()
    {
        InitializeComponent();
        _client = SslClientHelper.GetService<SslClientService>();
        _client.MessageReceived += OnMessageReceived;
    }

    private async void OnMessageReceived(string msg)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                try
                {
                    JsonDocument.Parse(msg);
                }
                catch(Exception ex)
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
                        if (error == "changedpassword_failed;pass_is_repeated")
                            await ShowAlert("Error", $"{msg}\nPassword is repeated");
                        else if (error == "changedpassword_failed;unaccess_token")
                            await ShowAlert("Error", $"{msg}\nU're bitch  ЪУЪ");
                        return;

                    case "success":
                        packet.body.TryGetValue("success", out string success);
                        success = success?.Trim().ToLower();
                        if (success == "changedpassword_success;pass_changed")
                        {
                            await ShowAlert("SUCCESS", $"{msg}\nTry to login with new password now");
                            await Task.Delay(100); // чтобы UI успел обновиться
                            await CloseAsync();
                        }
                        break;
                    default: return;
                }
            }
            catch (Exception ex)
            {
                await ShowAlert("Ошибка", ex.Message);
                return;

            }
        });
    }

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        string newPass = NewPasswordEntry.Text?.Trim();
        string repeatPass = RepeatPasswordEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(newPass) || string.IsNullOrWhiteSpace(repeatPass))
        {
            await ShowAlert("Error", "All args isn't filled");
            return;
        }

        if (newPass != repeatPass)
        {
            await ShowAlert("Error", "Passwords must be одинаковые");
            return;
        }

        try
        {
            await _client.SendJsonAsync("changedpass_data;", new() { ["password"] = newPass });
            await Task.Delay(500);
            //await ShowAlert("SUCCESS" , "Try to login with new password now");
            //await CloseAsync();
        }
        catch (Exception ex) { await ShowAlert("Error", ex.Message); }
    }

    private async Task ShowAlert(string title, string message)
    {
        await Application.Current.MainPage.DisplayAlertAsync(title, message, "OK");
    }
}