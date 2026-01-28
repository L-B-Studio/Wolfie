using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.ApplicationModel;
using System.Text;
using System.Text.Json;
using Wolfie.Helpers;
using Wolfie.Models;
using Wolfie.Models.DTOObjects.ChangePassword;
using Wolfie.Servers;
using Wolfie.Services;

namespace Wolfie.Popups;

public partial class ChangedPasswordPopup : Popup
{
    private readonly DeviceInfoHelper _deviceinfo;
    private string _reset_token;
    private readonly HttpClientService _httpClient;
    private readonly string uri = "auth/changedpass";
    public ChangedPasswordPopup(string reset_token = "none_token_getted")
    {
        InitializeComponent();
        _reset_token = reset_token;
        ApplyTheme();
        _httpClient = ServiceClientHelper.GetService<HttpClientService>();
        _deviceinfo = ServiceClientHelper.GetService<DeviceInfoHelper>();
    }

    // Event handler for confirm button click
    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        string newPass = NewPasswordEntry.Text.Trim();
        string repeatPass = RepeatPasswordEntry.Text.Trim();

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

        // Send password change request to server
        try
        {
            var changepass_dto = new ChangePasswordRequest
            {
                Token_reset = _reset_token,
                Password = newPass,
                Device_id = _deviceinfo.GetDeviceManufacture(),
                Device_type = _deviceinfo.GetDeviceType()
            };

            var answer = await _httpClient.SendChangePassRequestAsync(uri, changepass_dto);
            
            await CloseAsync();
        }
        catch (Exception ex) { await ShowAlert("Error", ex.Message); }
    }

    // method to show alert
    private async Task ShowAlert(string title, string message)
    {
        await Application.Current.MainPage.DisplayAlertAsync(title, message, "OK");
    }

    // method to apply theme
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
            EnterLabel.TextColor = Color.FromArgb("#E6E6E6");
            TitleLabel.TextColor = Color.FromArgb("#E6E6E6");
            MainLabel.TextColor = Color.FromArgb("#E6E6E6");


            //Buttons
            SubmitButton.BackgroundColor = Color.FromArgb("#2980FF");
            SubmitButton.TextColor = Colors.White;
           

            //Entries
            NewPasswordEntry.BackgroundColor = Color.FromArgb("#1D2633");
            NewPasswordEntry.PlaceholderColor = Color.FromArgb("#8E9BAA");
            RepeatPasswordEntry.BackgroundColor = Color.FromArgb("#1D2633");
            RepeatPasswordEntry.PlaceholderColor = Color.FromArgb("#8E9BAA");

            //Entries' Text
            NewPasswordEntry.TextColor = Color.FromArgb("#E6E6E6");
            RepeatPasswordEntry.TextColor = Color.FromArgb("#E6E6E6");

        }
        else
        {
            //photos
            Logo.Source = "logo.png";

            //page's background
            MainLayout.BackgroundColor = Color.FromArgb("#F4F7FA");
            BorderLayout.BackgroundColor = Color.FromArgb("#F4F7FA");

            //Labels
            EnterLabel.TextColor = Color.FromArgb("#1A1A1A");
            TitleLabel.TextColor = Color.FromArgb("#1A1A1A");
            MainLabel.TextColor = Color.FromArgb("#1A1A1A");

            //Buttons
            SubmitButton.BackgroundColor = Color.FromArgb("#2979FF");
            SubmitButton.TextColor = Colors.White;

            //Entries
            NewPasswordEntry.BackgroundColor = Color.FromArgb("#E8ECF1");
            NewPasswordEntry.PlaceholderColor = Color.FromArgb("#4A5058");
            RepeatPasswordEntry.BackgroundColor = Color.FromArgb("#E8ECF1");
            RepeatPasswordEntry.PlaceholderColor = Color.FromArgb("#4A5058");

            //Entries' Text
            NewPasswordEntry.TextColor = Color.FromArgb("#1A1A1A");
            RepeatPasswordEntry.TextColor = Color.FromArgb("#1A1A1A");

        }
    }
}