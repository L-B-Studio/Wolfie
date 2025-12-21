using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.ApplicationModel;
using System.Text.Json;
using Wolfie.Helpers;
using Wolfie.Models;
using Wolfie.Services;

namespace Wolfie.Popups;

public partial class ChangedPasswordPopup : Popup
{
    private readonly SslClientService _client;
    private string _reset_token;
    public ChangedPasswordPopup(string reset_token = "none_token_getted")
    {
        InitializeComponent();
        _reset_token = reset_token;
        ApplyTheme();
        _client = ServiceClientHelper.GetService<SslClientService>();
        Opened += OnPopupOpened;
        Closed += OnPopupClosed;
    }

    private void OnPopupOpened(object? sender, EventArgs e)
    {
        _client.MessageReceived += OnMessageReceived;
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        _client.MessageReceived -= OnMessageReceived;
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
                        if (error == "changedpassword_failed;pass_is_repeated")
                            await ShowAlert("Error", $"{msg}\nPassword is repeated");
                        else if (error == "invalid_or_expired_token")
                            await ShowAlert("Error", $"{msg}\nU're bitch  ЪУЪ");
                        return;

                    case "success":
                        packet.body.TryGetValue("success", out string success);
                        success = success?.Trim().ToLower();
                        if (success == "change_success;password_changed")
                        {
                            await ShowAlert("SUCCESS", $"{msg}\nTry to login with new password now");
                            await Task.Delay(100); // чтобы UI успел обновиться
                            await CloseAsync();
                        }
                        break;
                    default: await ShowAlert("Default message" , msg); return;
                }
            }
            catch (Exception ex)
            {
                await ShowAlert("Ошибка", ex.Message);
                return;

            }
        });
    }

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        string newPass = NewPasswordEntry.Text?.Trim();
        string repeatPass = RepeatPasswordEntry.Text?.Trim();

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

        try
        {

            await _client.SendJsonAsync("changedpass_data", new() 
            { 
                ["token_reset"] = _reset_token,
                ["password"] = newPass
            });
            await Task.Delay(500);
        }
        catch (Exception ex) { await ShowAlert("Error", ex.Message); }
    }

    private async Task ShowAlert(string title, string message)
    {
        await Application.Current.MainPage.DisplayAlertAsync(title, message, "OK");
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