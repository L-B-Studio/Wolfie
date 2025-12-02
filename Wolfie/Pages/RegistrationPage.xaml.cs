using CommunityToolkit.Maui.Extensions;
using System.Text.Json;
using Wolfie.Helpers;
using Wolfie.Models;
using Wolfie.Popups;
using Wolfie.Services;

namespace Wolfie.Pages;

public partial class RegistrationPage : ContentPage
{
    private bool _isDarktheme = false;
    private readonly SslClientService _tcpService;
    public RegistrationPage()
    {
        InitializeComponent();
        _tcpService = SslClientHelper.GetService<SslClientService>();
        _tcpService.MessageReceived += OnMessageReceived;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // ✅ Отписываемся во избежание утечки памяти/двойных событий
        _tcpService.MessageReceived -= OnMessageReceived;
    }

    private async void RegistrationButtonClicked(object sender, EventArgs e)
    {
        string username = NicknameEntry.Text?.Trim();
        string email = EmailEntry.Text?.Trim();
        string createPass = PasswordCreateEntry.Text;
        string againPass = PasswordAgainEntry.Text;
        DateTime birthday = BirthdayDatePicker.Date ?? DateTime.Now;

        //string dataMessage = $"registration_data;{username};{email};{createPass};{birthday:yyyy-MM-dd}";

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(createPass) ||
            string.IsNullOrWhiteSpace(againPass))
        {
            await DisplayAlertAsync("Erorr", "All rows must be filled", "Ок");
            return;
        }

        if (!IsValidBirthdayDate(birthday))
        {
            await DisplayAlertAsync("Error", "Write a normal date of birthday (from 3y.o to 120y.o)", "Ок");
            return;
        }

        if (createPass != againPass)
        {
            await DisplayAlertAsync("Error", "Passwords are different", "Ок");
            return;
        }

        if (createPass.Length < 6)
        {
            await DisplayAlertAsync("Error", "Password must be longer than 6 charapters", "Ок");
            return;
        }

        if (username.Length < 3)
        {
            await DisplayAlertAsync("Error", "Name of user must be longer than 3 charapters", "Ок");
            return;
        }

        if (!IsValidEmail(email))
        {
            await DisplayAlertAsync("Error", "Write real email", "Ок");
            return;
        }

        if (!AgreeCheckBox.IsChecked)
        {
            await DisplayAlertAsync("Error", "U must be agree with Privacy and Terms Rules", "Ок");
            return;
        }

        try
        {
            await _tcpService.SendJsonAsync("registration_data", new() {
                ["username"]= username,
                ["email"] = email,
                ["password"] = createPass,
                ["birthday"] = birthday.ToString()
            });
            await Task.Delay(500);
        }
        catch(Exception ex)
        {
            await DisplayAlertAsync("Error" , ex.Message , "Ок");
        }
    }

    private async void OnMessageReceived(string msg)
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
                    await DisplayAlertAsync("Error", ex.Message, "Ok");
                    return;
                }

                var packet = JsonSerializer.Deserialize<JsonPackage>(msg);
                if (packet == null || string.IsNullOrWhiteSpace(packet.header)) return;
                if (packet.body == null) packet.body = new Dictionary<string, string>();


                switch (packet.header.ToLower().Trim())
                {
                    case "error":
                        packet.body.TryGetValue("error", out string error);
                        error = error?.Trim().ToLower();
                        if (error == "registration_failed;email_exists")
                        { await DisplayAlertAsync("Error", "email have been registered yearlier", "Ок"); }
                        else if (error == "registration_failed;email_not_found")
                        {
                            await DisplayAlertAsync("Error", "It's unreal email , pls register real email", "Ок");
                        }
                        return;
                    case "success":
                        packet.body.TryGetValue("success", out string sucess);
                        sucess = sucess?.Trim().ToLower();
                        if (sucess == "registration_success;ok")
                        {
                            var popup = new EmailCodeVerifPopup(email , true);
                            await this.ShowPopupAsync(popup);
                            await Task.Delay(100);
                        }
                        break;
                    default: return;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", ex.Message, "OK");
                return;
            }
        });
    }

    public static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidBirthdayDate(DateTime dob)
    {
        var today = DateTime.Today;
        var age = today.Year - dob.Year;
        if (dob > today.AddYears(-age)) age--;
        return age >= 3 && age <= 120;
    }

    async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage());
    }

    private void OnThemeClicked(object sender, EventArgs e)
    {
        _isDarktheme = !_isDarktheme;

        if (_isDarktheme)
        {
            Application.Current.UserAppTheme = AppTheme.Dark;
            ThemeButton.Source = "sun.png";
            BackButton.Source = "back_white_button.png";
            Logo.Source = "light_logo.png";
            MainLayout.BackgroundColor = Colors.Black;

            TitleLabel.TextColor = Colors.White;
            //SubTitleLabel.TextColor = Colors.White;
            AgreeSpan.TextColor = Colors.White;
            AndSpan.TextColor = Colors.White;
            RegistrationButton.BackgroundColor = Colors.White;
            RegistrationButton.TextColor = Colors.Black;

            //CreateAccountLabel.TextColor = Colors.White;
            //NicknameLabel.TextColor = Colors.White;
            //BirthdayLabel.TextColor = Colors.White;
            //EmailLabel.TextColor = Colors.White;
            //PassLabel.TextColor = Colors.White;
        }
        else
        {
            Application.Current.UserAppTheme = AppTheme.Light;
            ThemeButton.Source = "moon.png";
            BackButton.Source = "back_button.png";
            Logo.Source = "logo.png";
            MainLayout.BackgroundColor = Colors.White;

            TitleLabel.TextColor = Colors.Black;
            //SubTitleLabel.TextColor = Colors.Black;
            AgreeSpan.TextColor = Colors.Black;
            AndSpan.TextColor = Colors.Black;
            RegistrationButton.BackgroundColor = Colors.Black;
            RegistrationButton.TextColor = Colors.White;

            //CreateAccountLabel.TextColor = Colors.Black;
            //NicknameLabel.TextColor = Colors.Black;
            //BirthdayLabel.TextColor = Colors.Black;
            //EmailLabel.TextColor = Colors.Black;
            //PassLabel.TextColor = Colors.Black;
        }
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

}