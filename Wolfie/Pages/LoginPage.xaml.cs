using CommunityToolkit.Maui.Extensions;
using System.Net.Mail;
using Wolfie.Helpers;
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
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {

            switch (msg)
            {
                case "ERROR;LOGIN_FAILED;INVALID_CREDENTIALS":
                    await DisplayAlertAsync("Ошибка", "Почта или пароль не правильные", "Ок");
                    break;
                default:
                    await DisplayAlertAsync("Успех", "Регистрация выполнена!", "Войти");
                    //var popup = new EmailCodeVerifPopup();
                    //await this.ShowPopupAsync(popup);
                    break;
            }
        });
    }

    private async void LoginButtonClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim();
        string password = PasswordEntry.Text;
        string dataMessage = $"login_data ; {email} ; {password}";

        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlertAsync("Error", "Все поля должны быть заполнены", "ok");
            return;
        }

        if (!IsValidEmail(email))
        {
            await DisplayAlertAsync("Error", "Напишите существующую почту", "ok");
            return;
        }

        if (!AgreeCheckBox.IsChecked)
        {
            await DisplayAlertAsync("Ошибка", "согласитесь с правилами и политикой", "ок");
            return;
        }
        try
        {
            await _tcpService.SendAsync(dataMessage);
        }
        catch (Exception er)
        {
            await DisplayAlertAsync("Ошибка", er.Message, "ОК");
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
            SubTitleLabel.TextColor = Colors.White;
            AgreeSpan.TextColor = Colors.White;
            AndSpan.TextColor = Colors.White;
            LoginButton.BackgroundColor = Colors.White;
            LoginButton.TextColor = Colors.Black;
            RegistrationButton.BackgroundColor = Colors.White;
            RegistrationButton.TextColor = Colors.Black;

            EnterAccountLabel.TextColor = Colors.White;
            EmailLabel.TextColor = Colors.White;
            PassLabel.TextColor = Colors.White;
            OrLable.TextColor = Colors.White;
        }
        else
        {
            Application.Current.UserAppTheme = AppTheme.Light;
            ThemeButton.Source = "moon.png";
            Logo.Source = "logo.png";
            MainLayout.BackgroundColor = Colors.White;

            TitleLabel.TextColor = Colors.Black;
            SubTitleLabel.TextColor = Colors.Black;
            AgreeSpan.TextColor = Colors.Black;
            AndSpan.TextColor = Colors.Black;
            LoginButton.BackgroundColor = Colors.Black;
            LoginButton.TextColor = Colors.White;
            RegistrationButton.BackgroundColor = Colors.Black;
            RegistrationButton.TextColor = Colors.White;

            EnterAccountLabel.TextColor = Colors.Black;
            EmailLabel.TextColor = Colors.Black;
            PassLabel.TextColor = Colors.Black;
            OrLable.TextColor = Colors.Black;
        }

    }

    //async void RegistrationButtonClicked(object sender, EventArgs e)
    //{
    //    await Shell.Current.GoToAsync(nameof(RegistrationPage));
    //}

    //private async void OnTermsTapped(object sender, EventArgs e)
    //{
    //    var popup = new TermsPopup();
    //    await this.ShowPopupAsync(popup);
    //}

    //private async void OnPrivacyTapped(object sender, EventArgs e)
   // {
    //    var popup = new PrivacyPopup();
    //    await this.ShowPopupAsync(popup);
   // }

    //private async void ForgotPasswordCkicked(object sender, EventArgs e)
    //{
    //    var popup = new ForgotPassPopup();
    //    await this.ShowPopupAsync(popup);
    //}
}