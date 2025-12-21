using CommunityToolkit.Maui.Extensions;
using System.Text.Json;
using Wolfie.Auth;
using Wolfie.Helpers;
using Wolfie.Models;
using Wolfie.Popups;
using Wolfie.Services;

namespace Wolfie.Pages;

public partial class RegistrationPage : ContentPage
{
    private readonly SslClientService _tcpService;
    private readonly AuthState _authstate;
    private readonly string _deviceInfo;
    public RegistrationPage()
    {
        InitializeComponent();
        _tcpService = ServiceClientHelper.GetService<SslClientService>();
        _authstate = ServiceClientHelper.GetService<AuthState>() ;
        _deviceInfo = DeviceInfoHelper.GetAllDeviceInfo();

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _tcpService.MessageReceived += OnMessageReceived;
        ApplyTheme();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _tcpService.MessageReceived -= OnMessageReceived;
    }

    private async void RegistrationButtonClicked(object sender, EventArgs e)
    {
        string username = NicknameEntry.Text?.Trim();
        string email = EmailEntry.Text?.Trim();
        string createPass = PasswordCreateEntry.Text;
        string againPass = PasswordAgainEntry.Text;
        DateTime birthday = BirthdayDatePicker.Date ?? DateTime.Now;

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
                ["birthday"] = birthday.ToString("MM-dd-yyyy"),
                ["device_info"] = _deviceInfo
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

                var packet = JsonSerializer.Deserialize<ServerJsonPackage>(msg);
                if (packet == null || string.IsNullOrWhiteSpace(packet.header)) return;
                if (packet.body == null) packet.body = new Dictionary<string, string>();


                switch (packet.header.ToLower().Trim())
                {
                    case "error":
                        packet.body.TryGetValue("error", out string error);
                        error = error?.Trim().ToLower();
                        if (error == "registration_failed;email_exists")
                        { await DisplayAlertAsync("Error", $"{msg}\nemail have been registered yearlier", "Ок"); }
                        else if (error == "registration_failed;email_not_found")
                        {
                            await DisplayAlertAsync("Error", $"{msg}\nIt's unreal email , pls register real email", "Ок");
                        }
                        return;
                    case "success":
                        try
                        {
                            packet.body.TryGetValue("token_refresh", out string refresh_token);
                            refresh_token = refresh_token?.Trim();
                            await SecureStorage.SetAsync("refresh_token", refresh_token);
                            await DisplayAlertAsync("refresh reg token", refresh_token, "ok");
                        }
                        catch (Exception ex)
                        {
                            await DisplayAlertAsync("Error in refresh token", ex.Message, "ok");
                        }
                        try
                        {
                            packet.body.TryGetValue("token_access", out string access_token);
                            access_token = access_token?.Trim();
                            _authstate.AccessToken = access_token;
                            await DisplayAlertAsync("access reg token", access_token, "ok");
                        }
                        catch (Exception ex)
                        {
                            await DisplayAlertAsync("Error in access token", ex.Message, "ok");
                        }
                        packet.body.TryGetValue("success", out string sucess);
                        sucess = sucess?.Trim().ToLower();
                        if (sucess == $"registration_success;ok")
                        {
                            //var popup = new EmailCodeVerifPopup(email , true);
                            //await this.ShowPopupAsync(popup);
                            await DisplayAlertAsync("SUCCESS", $"{msg}\nYou have registered!", "Enter");
                            await Shell.Current.GoToAsync(nameof(ChatListPage));
                            await Task.Delay(100);
                            break;
                        }
                        break;
                    default:
                        await DisplayAlertAsync("NONE" , msg , "ok");
                        return;

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
            await Shell.Current.GoToAsync("..");

    }

    private void ApplyTheme()
    {
        if (ThemeService.IsDarkTheme)
        {
            //page's background
            MainLayout.BackgroundColor = Color.FromArgb("#121821");

            //photos
            BackButton.Source = "back_white_button.png";
            Logo.Source = "light_logo.png";
            ThemeButton.Source = "sun.png";

            //labels
            TitleLabel.TextColor = Color.FromArgb("#E6E6E6");
            CreateLabel.TextColor = Color.FromArgb("#E6E6E6");
            AgreeText.TextColor = Color.FromArgb("#D8D8D8");

            //buttons
            RegistrationButton.BackgroundColor = Color.FromArgb("#2980FF");
            RegistrationButton.TextColor = Colors.White;

            //entries
            NicknameEntry.BackgroundColor = Color.FromArgb("#1D2633");
            NicknameEntry.PlaceholderColor = Color.FromArgb("#8E9BAA");
            BirthdayDatePicker.BackgroundColor = Color.FromArgb("#1D2633");
            BirthdayDatePicker.TextColor = Color.FromArgb("#E6E6E6");
            EmailEntry.BackgroundColor = Color.FromArgb("#1D2633");
            EmailEntry.PlaceholderColor = Color.FromArgb("#8E9BAA");
            PasswordCreateEntry.BackgroundColor = Color.FromArgb("#1D2633");
            PasswordCreateEntry.PlaceholderColor = Color.FromArgb("#8E9BAA");
            PasswordAgainEntry.BackgroundColor = Color.FromArgb("#1D2633");
            PasswordAgainEntry.PlaceholderColor = Color.FromArgb("#8E9BAA");

            //entries' text
            NicknameEntry.TextColor = Color.FromArgb("#E6E6E6");
            BirthdayDatePicker.TextColor = Color.FromArgb("#E6E6E6");
            EmailEntry.TextColor = Color.FromArgb("#E6E6E6");
            PasswordCreateEntry.TextColor = Color.FromArgb("#E6E6E6");
            PasswordAgainEntry.TextColor = Color.FromArgb("#E6E6E6");

            //Links
            TermsLink.TextColor = Color.FromArgb("#5DA3FA");
            PrivacyLink.TextColor = Color.FromArgb("#5DA3FA");
        }
        else
        {
            //page's background
            MainLayout.BackgroundColor = Color.FromArgb("#F4F7FA");

            //photos
            BackButton.Source = "back_button.png";
            Logo.Source = "logo.png";
            ThemeButton.Source = "moon.png";

            //labels
            TitleLabel.TextColor = Color.FromArgb("#1A1A1A");
            CreateLabel.TextColor = Color.FromArgb("#1A1A1A");
            AgreeText.TextColor = Color.FromArgb("#1A1A1A");

            //buttons
            RegistrationButton.BackgroundColor = Color.FromArgb("#2979FF");
            RegistrationButton.TextColor = Colors.White;

            //entries
            NicknameEntry.BackgroundColor = Color.FromArgb("#E8ECF1");
            NicknameEntry.PlaceholderColor = Color.FromArgb("#4A5058");
            BirthdayDatePicker.BackgroundColor = Color.FromArgb("#E8ECF1");
            BirthdayDatePicker.TextColor = Colors.Black;
            EmailEntry.BackgroundColor = Color.FromArgb("#E8ECF1");
            EmailEntry.PlaceholderColor = Color.FromArgb("#4A5058");
            PasswordCreateEntry.BackgroundColor = Color.FromArgb("#E8ECF1");
            PasswordCreateEntry.PlaceholderColor = Color.FromArgb("#4A5058");
            PasswordAgainEntry.BackgroundColor = Color.FromArgb("#E8ECF1");
            PasswordAgainEntry.PlaceholderColor = Color.FromArgb("#4A5058");

            //entries' text
            NicknameEntry.TextColor = Color.FromArgb("#1A1A1A");
            BirthdayDatePicker.TextColor = Color.FromArgb("#1A1A1A");
            EmailEntry.TextColor = Color.FromArgb("#1A1A1A");
            PasswordCreateEntry.TextColor = Color.FromArgb("#1A1A1A");
            PasswordAgainEntry.TextColor = Color.FromArgb("#1A1A1A");

            //Links
            TermsLink.TextColor = Color.FromArgb("#1565C0");
            PrivacyLink.TextColor = Color.FromArgb("#1565C0");
        }
    }

    private async void OnThemeClicked(object sender, EventArgs e)
    {
        var currentPage = Shell.Current.CurrentPage;

        if (currentPage != null)
        {
            // 1. Уходим в прозрачность
            await currentPage.FadeToAsync(0, 250, Easing.Linear);

            // 2. Меняем тему
            ThemeService.ToggleTheme();
            ApplyTheme();

            // 3. Возвращаем видимость
            await currentPage.FadeToAsync(1, 250, Easing.Linear);
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