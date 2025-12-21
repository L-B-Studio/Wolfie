using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using System.Text.Json;
using System.Xml.Linq;
using Wolfie.Helpers;
using Wolfie.Models;
using Wolfie.Pages;
using Wolfie.Services;

namespace Wolfie.Popups;

public partial class ForgotPassPopup : Popup
{
    private readonly SslClientService _tcpService;
    public ForgotPassPopup()
    {
        InitializeComponent();
        ApplyTheme();
        _tcpService = ServiceClientHelper.GetService<SslClientService>();
        Opened += OnPopupOpened;
        Closed += OnPopupClosed;
    }

    private void OnPopupOpened(object? sender, EventArgs e)
    {
        _tcpService.MessageReceived += OnMessageReceivedAsync;
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        _tcpService.MessageReceived -= OnMessageReceivedAsync;
    }

    private async void OnMessageReceivedAsync(string msg)
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
                        if (error == "forgotpass_failed;email_not_found")
                            await ShowAlert("Error", $"{msg}\nThis email is unregistered");
                        return;

                    case "success":
                        packet.body.TryGetValue("success", out string success);
                        success = success?.Trim().ToLower();
                        if (success == "forgotpass_success;confirmation_code_sent")
                        {
                            await CloseAsync();
                            var newPopup = new EmailCodeVerifPopup(email);
                            await Task.Delay(100); 
                            await Application.Current.MainPage.ShowPopupAsync(newPopup);
                        }
                        break;
                    default: return;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"MessageReceived error: {ex}");

                await ShowAlert("Ошибка", ex.Message);
                return;
            }
        });
    }


    private async void SendCodeButtonClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim();
        string dataMessage = $"forgotpass_data ; {email}";

        if (string.IsNullOrEmpty(email))
        {
            await ShowAlert("Error", "All rows must be filled");
            return;
        }
        try
        {
            await _tcpService.SendJsonAsync("forgotpass_data", new() { ["email"] = email });
            await Task.Delay(500);
        }
        catch (Exception ex)
        {
            await ShowAlert("Error: ", ex.Message);
        }

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

