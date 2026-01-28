using CommunityToolkit.Maui.Extensions;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using Wolfie.Auth;
using Wolfie.Helpers;
using Wolfie.Models;
using Wolfie.Models.DTOObjects;
using Wolfie.Models.DTOObjects.GetAccess;
using Wolfie.Models.DTOObjects.Login;
using Wolfie.Popups;
using Wolfie.Servers;
using Wolfie.Services;

namespace Wolfie.Pages;

public partial class LoginPage : ContentPage
{
    private readonly DeviceInfoHelper _deviceInfo;
    private readonly AuthState _authstate;
    private readonly HttpClientService _httpClient;
    private readonly string uri = "auth/login/";
    private readonly string isloggined_uri = "auth/get_access_token/";
    public LoginPage(DeviceInfoHelper deviceInfo, AuthState authState, HttpClientService httpClient)
    {
        InitializeComponent();

        // Тепер сервіси приходять готові від DI-контейнера
        _deviceInfo = deviceInfo;
        _authstate = authState;
        _httpClient = httpClient;

        ApplyTheme();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Используем задержку, чтобы навигация не конфликтовала с отрисовкой текущей страницы
        await Task.Delay(100);
        await CheckAuthAndNavigateAsync();
    }

    private async Task CheckAuthAndNavigateAsync()
    {
        try
        {
            var refreshToken = await SecureStorage.GetAsync("token_refresh");

            if (string.IsNullOrWhiteSpace(refreshToken))
                return;

            await _httpClient.InitializeAsync();

            var dto = new GetAccessTokenRequest
            {
                token_refresh = refreshToken,
                Device_id = _deviceInfo.GetDeviceManufacture(),
                Device_type = _deviceInfo.GetDeviceType()
            };

            var response = await _httpClient.GetNewAccessToken(isloggined_uri, dto);


            if (response?.token_access != null)
            {
                _authstate.AccessToken = response.token_access;

                if (!string.IsNullOrEmpty(response.token_refresh))
                    await SecureStorage.SetAsync("token_refresh", response.token_refresh);

                // Переключаемся в главный поток ТОЛЬКО для UI операций
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    // Важно: проверьте, существует ли ChatListPage в AppShell.xaml
                    await Shell.Current.GoToAsync(nameof(ChatListPage));
                });
            }

            else
            {
                SecureStorage.Remove("token_refresh");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CRITICAL ERROR: {ex}");
        }
    }


    private async void LoginButtonClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text.Trim();
        string pass = PasswordEntry.Text;



        if (string.IsNullOrWhiteSpace(pass) || string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlertAsync("Error in pass", "All rows must be filled", "ok");
            return;
        }

        if (!IsValidEmail(email))
        {
            await DisplayAlertAsync("Error in email", "Write real email", "ok");
            return;
        }

        if (!AgreeCheckBox.IsChecked)
        {
            await DisplayAlertAsync("Error in rules", "Agree with Terms and Privacy rules", "ок");
            return;
        }
        // Send data to server
        try
        {

            var user_login = new LoginRequest
            {
                Email = email,
                Password = pass,
                Device_id = _deviceInfo.GetDeviceManufacture(),
                Device_type = _deviceInfo.GetDeviceType()
            };

            var answer = await _httpClient.SendLoginRequestAsync(uri, user_login);

            // FIX: Check if answer is null before processing
            if (answer == null)
            {
                await DisplayAlertAsync("Connection Error", "Failed to connect to server. Please check your internet or server status.", "OK");
                return;
            }

            // Process server response
            var boleanResult = await CheckLoginAnswerAsync(answer);
            if (boleanResult)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlertAsync("Success", "Login completed successfully!", "Ок");
                    try
                    {
                        await Shell.Current.GoToAsync(nameof(ChatListPage));
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlertAsync("Navigation Error", ex.Message, "Ок");
                    }
                });
            }
            await Task.Delay(500);
        }
        catch (Exception er)
        {
            await DisplayAlertAsync("Exception in send", er.Message, "ОК");
        }
    }

    // Method to check server response
    private async Task<bool> CheckLoginAnswerAsync(LoginResponse answer)
    {
        if (answer.token_access != null)
        {
            System.Diagnostics.Debug.WriteLine($"Refresh_token {answer.token_refresh}");
            await SecureStorage.SetAsync("token_refresh", answer.token_refresh);
            _authstate.AccessToken = answer.token_access;
            return true;
        }
        else
        {
            string errorMessage = "Refresh token is null.";
            await DisplayAlertAsync("Login Failed", errorMessage, "OK");
            return false;
        }
    }


    // Email validation method
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

    // Event handler for theme toggle button click
    private async void OnThemeClicked(object sender, EventArgs e)
    {
        var currentPage = Shell.Current.CurrentPage;

        if (currentPage != null)
        {
            // 1. get to transparent
            await currentPage.FadeToAsync(0, 250, Easing.Linear);

            // 2. change theme
            ThemeService.ToggleTheme();
            ApplyTheme();

            // 3. back to opaque
            await currentPage.FadeToAsync(1, 250, Easing.Linear);
        }
    }

    // Apply theme based on current setting
    private void ApplyTheme()
    {
        if (ThemeService.IsDarkTheme)
        {
            //photos
            ThemeButton.Source = "sun.png";
            Logo.Source = "light_logo.png";

            //page's background 
            MainLayout.BackgroundColor = Color.FromArgb("#121821");

            //labels
            EnterLabel.TextColor = Color.FromArgb("#E6E6E6");
            TitleLabel.TextColor = Color.FromArgb("#E6E6E6");
            AgreeText.TextColor = Color.FromArgb("#D8D8D8");
            OrLabel.TextColor = Color.FromArgb("#7E8A97");

            //Buttons
            LoginButton.BackgroundColor = Color.FromArgb("#2980FF");
            LoginButton.TextColor = Colors.White;
            RegistrationButton.BackgroundColor = Color.FromArgb("#3C4E64");
            RegistrationButton.TextColor = Colors.White;

            //Entries
            EmailEntry.BackgroundColor = Color.FromArgb("#1D2633");
            EmailEntry.PlaceholderColor = Color.FromArgb("#8E9BAA");
            PasswordEntry.BackgroundColor = Color.FromArgb("#1D2633");
            PasswordEntry.PlaceholderColor = Color.FromArgb("#8E9BAA");

            //Entries' Text
            EmailEntry.TextColor = Color.FromArgb("#E6E6E6");
            PasswordEntry.TextColor = Color.FromArgb("#E6E6E6");

            //Links
            TermsLink.TextColor = Color.FromArgb("#5DA3FA");
            PrivacyLink.TextColor = Color.FromArgb("#5DA3FA");
        }
        else
        {
            //photos
            ThemeButton.Source = "moon.png";
            Logo.Source = "logo.png";

            //page's background
            MainLayout.BackgroundColor = Color.FromArgb("#F4F7FA");

            //Labels
            EnterLabel.TextColor = Color.FromArgb("#1A1A1A");
            TitleLabel.TextColor = Color.FromArgb("#1A1A1A");
            AgreeText.TextColor = Color.FromArgb("#1A1A1A");
            OrLabel.TextColor = Color.FromArgb("#1A1A1A");

            //Buttons
            LoginButton.BackgroundColor = Color.FromArgb("#2979FF");
            LoginButton.TextColor = Colors.White;
            RegistrationButton.BackgroundColor = Color.FromArgb("#2979FF");
            RegistrationButton.TextColor = Colors.White;

            //Entries
            EmailEntry.BackgroundColor = Color.FromArgb("#E8ECF1");
            EmailEntry.PlaceholderColor = Color.FromArgb("#4A5058");
            PasswordEntry.BackgroundColor = Color.FromArgb("#E8ECF1");
            PasswordEntry.PlaceholderColor = Color.FromArgb("#4A5058");

            //Entries' Text
            EmailEntry.TextColor = Color.FromArgb("#1A1A1A");
            PasswordEntry.TextColor = Color.FromArgb("#1A1A1A");

            //Links
            TermsLink.TextColor = Color.FromArgb("#1565C0");
            PrivacyLink.TextColor = Color.FromArgb("#1565C0");
        }
    }

    // Event handler for registration button click
    async void RegistrationButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RegistrationPage));
    }

    // Event handler for terms link tap
    private async void OnTermsTapped(object sender, EventArgs e)
    {
        var popup = new TermsPopup();
        await this.ShowPopupAsync(popup);
    }

    // Event handler for privacy link tap
    private async void OnPrivacyTapped(object sender, EventArgs e)
    {
        var popup = new PrivacyPopup();
        await this.ShowPopupAsync(popup);
    }

    // Event handler for forgot password link tap
    private async void ForgotPasswordCkicked(object sender, EventArgs e)
    {
        var popup = new ForgotPassPopup();
        await this.ShowPopupAsync(popup);
    }
}