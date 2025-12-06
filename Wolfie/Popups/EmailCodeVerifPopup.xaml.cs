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
    private string _email;
    private bool _fromRegister;

    public EmailCodeVerifPopup(string email , bool fromRegister = false)
    {
        InitializeComponent();
        _tcpService = SslClientHelper.GetService<SslClientService>();
        _tcpService.MessageReceived += OnMessageReceived;
        _email = email;
        _fromRegister = fromRegister;
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
                        //if (error == "cant_send_email")
                        //    await ShowAlert("Error", "This email is unreal");
                        if (error == "emailcode_failed;invalid_code")
                        {
                            await ShowAlert("Error", $"{msg}\nInvalid code");
                        }
                        if (error == "registration_failed;unaccess_token")
                            await ShowAlert("Error", $"{msg}\nU're bitch  ЪУЪ");
                        return;

                    case "success":
                        packet.body.TryGetValue("success", out string success);
                        success = success?.Trim().ToLower();
                        if (success == "forgotpass_success;email_verified")
                        {
                            var newPopup = new ChangedPasswordPopup();
                            await Task.Delay(100); // чтобы UI успел обновиться
                            await CloseAsync();
                            await Application.Current.MainPage.ShowPopupAsync(newPopup);
                        }
                        else if (success == "registration_success;email_verified")
                        {
                            await ShowAlert("SUCCESS", $"{msg}\nRegistration have been completed!" , "Enter");
                            return;
                        }
                        break;
                    default: return;
                }
            }
            catch (Exception ex)
            {
                await ShowAlert("Error", ex.Message);
                return;

            }
        });
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
            if (_fromRegister == true) 
            {
                await _tcpService.SendJsonAsync("register_verify_data", new() { ["email"] = _email, ["code"] = code });
                await Task.Delay(500);
            }
            else{
                await _tcpService.SendJsonAsync("forgotpass_verify_data", new() { ["email"] = _email, ["code"] = code });
                await Task.Delay(500);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlertAsync("Error", ex.Message, "Ok");
        }
    }

    private async Task ShowAlert(string title, string message , string end = "OK")
    {
        await Application.Current.MainPage.DisplayAlertAsync(title, message, end);
    }
}
