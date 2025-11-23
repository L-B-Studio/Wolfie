using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using System.Linq.Expressions;
using System.Text.Json;
using Wolfie.Helpers;
using Wolfie.Models;
using Wolfie.Services;

namespace Wolfie.Popups;

public partial class EmailCodeVerifPopup : Popup
{
    private readonly SslClientService _tcpService;
    private bool _isEditing = false;

    public EmailCodeVerifPopup()
    {
        InitializeComponent();
        _tcpService = SslClientHelper.GetService<SslClientService>();
        _tcpService.MessageReceived += OnMessageReceived;


    }

    private async void OnMessageReceived(string msg)
    {

        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (!IsJson(msg))
            {
                await ShowAlert("Error", "Dont get Json");
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
                    if (error == "cant_send_email")
                        await ShowAlert("Error", "This email is unreal");
                    else if (error == "invalid_code")
                        await ShowAlert("Error", "Invalid code");
                    return;

                case "success":
                    packet.body.TryGetValue("success", out string success);
                    success = success?.Trim().ToLower();
                    if (success == "email_verified")
                    {
                        var newPopup = new ChangedPasswordPopup();
                        await Task.Delay(100); // чтобы UI успел обновиться
                        await CloseAsync();
                        await Application.Current.MainPage.ShowPopupAsync(newPopup);
                    }
                    break;
                default: return;
            }
        });
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await ShowAlert("Ошибка", ex.Message);
                return;
            });
        }
    }

    private bool IsJson(string msg)
    {
        msg = msg.Trim();
        return (msg.StartsWith("{") && msg.EndsWith("}")) ||
               (msg.StartsWith("[") && msg.EndsWith("]"));
    }

    private void CodeEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isEditing) return;
        _isEditing = true;

        var entry = sender as Entry;
        string text = entry.Text ?? "";

        // Оставляем только буквы и цифры
        text = new string(text.Where(char.IsLetterOrDigit).ToArray());
        entry.Text = text;

        if (text.Length == 3)
        {
            if (entry == CodeEntry1) CodeEntry2.Focus();
            else if (entry == CodeEntry2) CodeEntry3.Focus();
        }
        else if (text.Length == 0)
        {
            if (entry == CodeEntry3) CodeEntry2.Focus();
            else if (entry == CodeEntry2) CodeEntry1.Focus();
        }

        _isEditing = false;
    }

    private async void VerifButtonClicked(object sender, EventArgs e)
    {
        string code = $"{CodeEntry1.Text}{CodeEntry2.Text}{CodeEntry3.Text}";

        if (code.Length != 9)
        {
            await Application.Current.MainPage.DisplayAlertAsync("Error", "Write all 3 nums in all 3 columns", "OK");
            return;
        }

        try
        {
            await _tcpService.SendJsonAsync("verify_data", new() { ["code"] = code });
            await Task.Delay(500);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlertAsync("Error", ex.Message, "Ok");
        }
    }

    private async Task ShowAlert(string title, string message)
    {
        await Application.Current.MainPage.DisplayAlertAsync(title, message, "OK");
    }
}
