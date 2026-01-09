using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using Wolfie.Helpers;
using Wolfie.Models;
using Wolfie.Models.DTOObjects.ForgotPassword;
using Wolfie.Servers;
using Wolfie.Services;

namespace Wolfie.Popups;

public partial class EmailCodeVerifPopup : Popup
{
    private readonly DeviceInfoHelper _deviceInfo;
    private bool _isEditing = false;
    private string _email;

    private readonly string uri = "auth/forgotpass_verify";
    private readonly HttpClientService _httpClient;

    public EmailCodeVerifPopup(string email)
    {
        InitializeComponent();
        ApplyTheme();

        _httpClient = ServiceClientHelper.GetService<HttpClientService>();

        _deviceInfo = ServiceClientHelper.GetService<DeviceInfoHelper>();
        _email = email;
    }

    // Event handler for TextChanged event of the Entry controls
    private void CodeEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isEditing) return;
        _isEditing = true;

        var entry = sender as Entry;
        string text = entry.Text ?? "";

        
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

    // Event handler for the Submit button click
    private async void VerifButtonClicked(object sender, EventArgs e)
    {
        string code = $"{CodeEntry1.Text}{CodeEntry2.Text}{CodeEntry3.Text}";
        if (code.Length != 9)
        {
            await Application.Current.MainPage.DisplayAlertAsync("Error", "Write all 3 nums in all 3 columns", "OK");
            return;
        }

        // Prepare data and send request to server
        try
        {
            var verifyemail_dto = new ForgotpassVerifyRequest
            {
                Email = _email,
                Code = code
                };


            var answer = await _httpClient.SendForgotpassVerifyRequestAsync(uri, verifyemail_dto);
            
            if(answer == null)
            {
                await ShowAlert("Error", "No response from server. Please try again later.");
            }
            //// Process server response
            var boleanResult = await CheckAnswerAsync(answer);
            if (boleanResult)
            {
                await CloseAsync();
                var newPopup = new ChangedPasswordPopup(answer.token_reset);
                await Task.Delay(100);
                await Application.Current.MainPage.ShowPopupAsync(newPopup);
            }
            await Task.Delay(500);
            
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlertAsync("Error", ex.Message, "Ok");
        }
    }

    // Method to check the server response
    private async Task<bool> CheckAnswerAsync(ForgotpassVerifyResponse answer)
    {
        if (answer.token_reset != null)
        {
            return true;
        }
        else
        {
            //string errorMessage = answer.body.ContainsKey("error") ? answer.body["error"] : "Unknown error occurred.";
            await ShowAlert("Change password -> Failed", "Reset token is null");
            return false;
        }
    }

    // Method to show alert messages
    private async Task ShowAlert(string title, string message, string end = "OK")
    {
        await Application.Current.MainPage.DisplayAlertAsync(title, message, end);
    }

    // Method to apply theming based on the current theme
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
