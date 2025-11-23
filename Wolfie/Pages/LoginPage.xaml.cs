using CommunityToolkit.Maui.Extensions;
using System.Net.Mail;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using Wolfie.Helpers;
using Wolfie.Models;
using Wolfie.Popups;
using Wolfie.Services;

namespace Wolfie.Pages;

public partial class LoginPage : ContentPage
{
    private readonly SslClientService _tcpService;
    private bool _isDarktheme = false;

    public LoginPage()
    {
        InitializeComponent();
        _tcpService = SslClientHelper.GetService<SslClientService>();
        _tcpService.MessageReceived += OnMessageReceived;

    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // ? Отписываемся во избежание утечки памяти/двойных событий
        _tcpService.MessageReceived -= OnMessageReceived;
    }

    private async void OnMessageReceived(string msg)
    {
        try
        {
            if (!IsJson(msg))
            {
                await DisplayAlertAsync("Ошибка", "Получены некорректные данные, не JSON", "Ок");
                return;
            }

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
                        if (error == "invalid_credentials")
                            await DisplayAlertAsync("Ошибка", "Почта или пароль неправильные", "Ок");
                        return;

                    case "success":
                        packet.body.TryGetValue("success", out string success);
                        success = success?.Trim().ToLower();
                        if (success == "log_ok")
                        {
                            await DisplayAlertAsync("SUCCESS", "You have logged in!", "Enter");
                            await Task.Delay(100);
                        }
                        break ;
                }
            });
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "OK");
            return;
        }
    }

    private bool IsJson(string msg)
    {
        msg = msg.Trim();
        return (msg.StartsWith("{") && msg.EndsWith("}")) ||
               (msg.StartsWith("[") && msg.EndsWith("]"));
    }



    private async void LoginButtonClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim();
        string pass = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(pass) || string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlertAsync("Error", "All rows must be filled", "ok");
            return;
        }

        if (!IsValidEmail(email))
        {
            await DisplayAlertAsync("Error", "Write real email", "ok");
            return;
        }

        if (!AgreeCheckBox.IsChecked)
        {
            await DisplayAlertAsync("Error", "Agree with Terms and Privacy rules", "ок");
            return;
        }
        try
        {
            await _tcpService.SendJsonAsync("login_data", new()
            {
                //["device model"] = DeviceInfo.Model,
                //["device name"] = DeviceInfo.Name
                ["email"] = email,
                ["password"] = pass
            });
            await Task.Delay(500);

        }
        catch (Exception er)
        {
            await DisplayAlertAsync("Error", er.Message, "ОК");
        }

    }

    public bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private void OnThemeClicked(object sender, EventArgs e)
    {
        _isDarktheme = !_isDarktheme;
        if (_isDarktheme)
        {
            Application.Current.UserAppTheme = AppTheme.Dark;
            ThemeButton.Source = "sun.png";

            Logo.Source = "light_logo.png";
            MainLayout.BackgroundColor = Colors.Black;

            TitleLabel.TextColor = Colors.White;
            //SubTitleLabel.TextColor = Colors.White;
            AgreeSpan.TextColor = Colors.White;
            AndSpan.TextColor = Colors.White;
            LoginButton.BackgroundColor = Colors.White;
            LoginButton.TextColor = Colors.Black;
            RegistrationButton.BackgroundColor = Colors.White;
            RegistrationButton.TextColor = Colors.Black;

            //EnterAccountLabel.TextColor = Colors.White;
            //EmailLabel.TextColor = Colors.White;
            //PassLabel.TextColor = Colors.White;
            OrLable.TextColor = Colors.White;
        }
        else
        {
            Application.Current.UserAppTheme = AppTheme.Light;
            ThemeButton.Source = "moon.png";
            Logo.Source = "logo.png";
            MainLayout.BackgroundColor = Colors.White;

            TitleLabel.TextColor = Colors.Black;
            //SubTitleLabel.TextColor = Colors.Black;
            AgreeSpan.TextColor = Colors.Black;
            AndSpan.TextColor = Colors.Black;
            LoginButton.BackgroundColor = Colors.Black;
            LoginButton.TextColor = Colors.White;
            RegistrationButton.BackgroundColor = Colors.Black;
            RegistrationButton.TextColor = Colors.White;

            //EnterAccountLabel.TextColor = Colors.Black;
            //EmailLabel.TextColor = Colors.Black;
            //PassLabel.TextColor = Colors.Black;
            OrLable.TextColor = Colors.Black;
        }

    }

    async void RegistrationButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RegistrationPage));
    }

    private async void OnTermsTapped(object sender, EventArgs e)
    {
        var popup = new TermsPopup();
        await this.ShowPopupAsync(popup);
    }
    
    private async void OnPrivacyTapped(object sender, EventArgs e)
    {
        var popup = new PrivacyPopup();
        await this.ShowPopupAsync(popup);
    }
    
    private async void ForgotPasswordCkicked(object sender, EventArgs e)
    {
        var popup = new ForgotPassPopup();
        await this.ShowPopupAsync(popup);
    }
}