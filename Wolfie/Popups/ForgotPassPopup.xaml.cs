using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Wolfie.Helpers;
using Wolfie.Models;
using Wolfie.Models.DTOObjects.ForgotPassword;
using Wolfie.Pages;
using Wolfie.Servers;
using Wolfie.Services;

namespace Wolfie.Popups;

public partial class ForgotPassPopup : Popup
{
    private readonly HttpClientService _httpClient;
    private readonly string uri = "auth/forgotpass";   

    public ForgotPassPopup()
    {
        InitializeComponent();
        ApplyTheme();

        _httpClient = ServiceClientHelper.GetService<HttpClientService>();
    }

    // Event handler for the Submit button click
    private async void SendCodeButtonClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim();


        if (string.IsNullOrEmpty(email))
        {
            await ShowAlert("Error", "All rows must be filled");
            return;
        }
        try
        {
            var forgotpass_dto = new ForgotPassRequest { Email = email };

            var answer = await _httpClient.SendForgotPassRequestAsync(uri,  forgotpass_dto);

            await CloseAsync();
            var popup = new EmailCodeVerifPopup(email);
            await Application.Current.MainPage.ShowPopupAsync(popup);
            await Task.Delay(500);
        }
        catch (Exception ex)
        {
            await ShowAlert("Error: ", ex.Message);
        }
    }

    // Method to show alert messages
    private async Task ShowAlert(string title, string message)
    {
        await Application.Current.MainPage.DisplayAlertAsync(title, message, "OK");
    }

    // Method to apply theme based on ThemeService
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
            MainLabel.TextColor = Color.FromArgb("#E6E6E6");
            EnterLabel.TextColor = Color.FromArgb("#E6E6E6");

            //Buttons
            SubmitButton.BackgroundColor = Color.FromArgb("#2980FF");
            SubmitButton.TextColor = Colors.White;


            //Entries
            EmailEntry.BackgroundColor = Color.FromArgb("#1D2633");
            EmailEntry.PlaceholderColor = Color.FromArgb("#8E9BAA");

            //Entries' Text
            EmailEntry.TextColor = Color.FromArgb("#E6E6E6");

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
            EnterLabel.TextColor = Color.FromArgb("#1A1A1A");

            //Buttons
            SubmitButton.BackgroundColor = Color.FromArgb("#2979FF");
            SubmitButton.TextColor = Colors.White;

            //Entries
            EmailEntry.BackgroundColor = Color.FromArgb("#E8ECF1");
            EmailEntry.PlaceholderColor = Color.FromArgb("#4A5058");

            //Entries' Text
            EmailEntry.TextColor = Color.FromArgb("#1A1A1A");
        }
    }
}

