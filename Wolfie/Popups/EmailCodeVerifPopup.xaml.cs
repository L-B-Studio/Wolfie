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
    //private readonly GetCurrentLocationService _location = new GetCurrentLocationService();
    private readonly string _deviceInfo;
    private bool _isEditing = false;
    private string _email;
    private bool _fromRegister;

    public EmailCodeVerifPopup(string email, bool fromRegister = false)
    {
        InitializeComponent();
        ApplyTheme();
        _tcpService = ServiceClientHelper.GetService<SslClientService>();
        _deviceInfo = DeviceInfoHelper.GetAllDeviceInfo();
        _email = email;
        _fromRegister = fromRegister;
        Opened += OnPopupOpened;
        Closed += OnPopupClosed;
    }

    private void OnPopupOpened(object? sender, EventArgs e)
    {
        _tcpService.MessageReceived += OnMessageReceived;
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        _tcpService.MessageReceived -= OnMessageReceived;
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
                var packet = JsonSerializer.Deserialize<ServerJsonPackage>(msg);
                if (packet == null || string.IsNullOrWhiteSpace(packet.header)) return;
                if (packet.body == null) packet.body = new Dictionary<string, string>();

                switch (packet.header.Trim().ToLower())
                {
                    case "error":
                        packet.body.TryGetValue("error", out string error);
                        error = error?.Trim().ToLower();
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
                        switch (success){
                            case "verify_code_confirm":
                                packet.body.TryGetValue("token_reset", out string reset_token);
                                reset_token = reset_token?.Trim();
                                await ShowAlert("reset token exception", reset_token);
                                if (reset_token != null)
                                {
                                    await CloseAsync();
                                    var newPopup = new ChangedPasswordPopup(reset_token);
                                    await Task.Delay(100); // чтобы UI успел обновиться
                                    await Application.Current.MainPage.ShowPopupAsync(newPopup);
                                }
                                else if (reset_token == "registration_success;email_verified")
                                {
                                    await ShowAlert("SUCCESS", $"{msg}\nRegistration have been completed!", "Enter");
                                    return;
                                }
                                break;
                            default:
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
        //var location = await _location.GetCurrentLocationAsync();
        //string deviceLocation = location != null ? $"{location.Latitude}, {location.Longitude}" : "Unknown";
        if (code.Length != 9)
        {
            await Application.Current.MainPage.DisplayAlertAsync("Error", "Write all 3 nums in all 3 columns", "OK");
            return;
        }

        try
        {
            if (_fromRegister == true)
            {
                await _tcpService.SendJsonAsync("register_verify_data", new()
                {
                    ["email"] = _email,
                    ["code"] = code,
                    ["device_info"] = _deviceInfo
                });
                await Task.Delay(500);
            }
            else
            {
                await _tcpService.SendJsonAsync("forgotpass_verify_data", new() { 
                    ["email"] = _email, 
                    ["code"] = code,
                    ["device_info"] = _deviceInfo
                });
                await Task.Delay(500);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlertAsync("Error", ex.Message, "Ok");
        }
    }

    private async Task ShowAlert(string title, string message, string end = "OK")
    {
        await Application.Current.MainPage.DisplayAlertAsync(title, message, end);
    }

    private void ApplyTheme()
    {
        if (ThemeService.IsDarkTheme)
        {
            //photos
            Logo.Source = "light_logo.png";

            //page's background 
            MainLayout.BackgroundColor = Color.FromArgb("#121821");
            BorderLayout.BackgroundColor = Color.FromArgb("#121821");

            //labels
            TitleLabel.TextColor = Color.FromArgb("#E6E6E6");
            MainLabel.TextColor = Color.FromArgb("#D8D8D8");


            //Buttons
            SubmitButton.BackgroundColor = Color.FromArgb("#2980FF");
            SubmitButton.TextColor = Colors.White;


            //Entries
            CodeEntry1.BackgroundColor = Color.FromArgb("#1D2633");
            CodeEntry1.PlaceholderColor = Color.FromArgb("#8E9BAA");
            CodeEntry2.BackgroundColor = Color.FromArgb("#1D2633");
            CodeEntry2.PlaceholderColor = Color.FromArgb("#8E9BAA");
            CodeEntry3.BackgroundColor = Color.FromArgb("#1D2633");
            CodeEntry3.PlaceholderColor = Color.FromArgb("#8E9BAA");

            //Entries' Text
            CodeEntry1.TextColor = Color.FromArgb("#E6E6E6");
            CodeEntry2.TextColor = Color.FromArgb("#E6E6E6");
            CodeEntry3.TextColor = Color.FromArgb("#E6E6E6");

        }
        else
        {
            //photos
            Logo.Source = "logo.png";

            //page's background
            MainLayout.BackgroundColor = Color.FromArgb("#F4F7FA");
            BorderLayout.BackgroundColor = Color.FromArgb("#F4F7FA");

            //Labels
            TitleLabel.TextColor = Color.FromArgb("#1A1A1A");
            MainLabel.TextColor = Color.FromArgb("#1A1A1A");

            //Buttons
            SubmitButton.BackgroundColor = Color.FromArgb("#2979FF");
            SubmitButton.TextColor = Colors.White;

            //Entries
            CodeEntry1.BackgroundColor = Color.FromArgb("#E8ECF1");
            CodeEntry1.PlaceholderColor = Color.FromArgb("#4A5058");
            CodeEntry2.BackgroundColor = Color.FromArgb("#E8ECF1");
            CodeEntry2.PlaceholderColor = Color.FromArgb("#4A5058");
            CodeEntry3.BackgroundColor = Color.FromArgb("#E8ECF1");
            CodeEntry3.PlaceholderColor = Color.FromArgb("#4A5058");

            //Entries' Text
            CodeEntry1.TextColor = Color.FromArgb("#1A1A1A");
            CodeEntry2.TextColor = Color.FromArgb("#1A1A1A");
            CodeEntry3.TextColor = Color.FromArgb("#1A1A1A");
        }
    }
}
