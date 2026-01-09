using CommunityToolkit.Maui.Extensions;
using System.Text;
using System.Text.Json;
using Wolfie.Auth;
using Wolfie.Helpers;
using Wolfie.Models;
using Wolfie.Models.DTOObjects.Login;
using Wolfie.Models.DTOObjects.Registration;
using Wolfie.Popups;
using Wolfie.Servers;
using Wolfie.Services;

namespace Wolfie.Pages;

public partial class RegistrationPage : ContentPage
{
    private readonly AuthState _authstate;
    private readonly HttpClientService _httpClient;
    private readonly DeviceInfoHelper _deviceInfo;
    private readonly string uri = "auth/registration";
    public RegistrationPage()
    {
        InitializeComponent();
        _authstate = ServiceClientHelper.GetService<AuthState>() ;
        _deviceInfo = ServiceClientHelper.GetService<DeviceInfoHelper>();
        _httpClient = ServiceClientHelper.GetService<HttpClientService>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyTheme();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }

    // Event handler for registration button click
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
        // Send registration data to server
        try
        {
            var registration_dto = new RegistrationRequest
            {
                Username = username,
                Email = email,
                Password = createPass,
                Birthday = birthday.ToString("yyyy-MM-dd"),
                Device_id = _deviceInfo.GetDeviceManufacture(),
                Device_type = _deviceInfo.GetDeviceType()
            };

            var answer = await _httpClient.SendRegistrationRequestAsync(uri, registration_dto);
           if (answer == null)
            {
                await DisplayAlertAsync("Connection Error", "Failed to connect to server. Please check your internet or server status.", "OK");
                return;
            }

            //// Process server response
            var booleanResult = await CheckAnswerAsync(answer);
            
            if (booleanResult)
            {
                await DisplayAlertAsync("Success", "Registration completed successfully!", "Ок");
                await Shell.Current.GoToAsync(nameof(ChatListPage));
            }

            await Task.Delay(500);
        }
        catch(Exception ex)
        {
            await DisplayAlertAsync("Error" , ex.Message , "Ок");
        }
    }

    // Method to check server response for registration
    private async Task<bool> CheckAnswerAsync(RegistrationResponse answer)
    {
        if (answer.token_access != null)
        {
            SecureStorage.SetAsync("token_refresh", answer.token_refresh);
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

    // Simple email validation method
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

    // Simple birthday date validation method
    private bool IsValidBirthdayDate(DateTime dob)
    {
        var today = DateTime.Today;
        var age = today.Year - dob.Year;
        if (dob > today.AddYears(-age)) age--;
        return age >= 3 && age <= 120;
    }

    // Event handler for back button click
    async void OnBackClicked(object sender, EventArgs e)
    {
            await Shell.Current.GoToAsync("..");

    }

    // Apply theme based on current theme setting
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

    // Event handler for theme toggle button click
    private async void OnThemeClicked(object sender, EventArgs e)
    {
        var currentPage = Shell.Current.CurrentPage;

        if (currentPage != null)
        {
            // 1. Get to transparent
            await currentPage.FadeToAsync(0, 250, Easing.Linear);

            // 2. change theme
            ThemeService.ToggleTheme();
            ApplyTheme();

            // 3. back to opaque
            await currentPage.FadeToAsync(1, 250, Easing.Linear);
        }
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

}