using CommunityToolkit.Maui.Views;
using System.Text;
using System.Text.Json;
using Wolfie.Helpers;
using Wolfie.Models;
using Wolfie.Services;

namespace Wolfie.Popups;

public partial class EmailCodeVerifPopup : Popup
{
    private readonly SslClientService _tcpService;
    private bool _isEditing = false;
    public EmailCodeVerifPopup()
    {
        InitializeComponent();
        _tcpService = SslClientHelper.GetService<SslClientService>();
        _tcpService.MessageReceived += OnMessageReceived;
        CodeEntry.TextChanged += CodeEntry_TextChanged;
    }

    private async void OnMessageReceived(string msg)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            _tcpService.MessageReceived -= OnMessageReceived;

            var packet = JsonSerializer.Deserialize<JsonPackage>(msg);
            switch (packet.header.ToLower().Trim())
            {
                case "error":
                    packet.body.TryGetValue("error", out string error);
                    error = error?.Trim().ToLower();
                    if (error == "cant_send_email")
                    {
                        await Application.Current.MainPage
                            .DisplayAlertAsync("Error", "This email is unreal" , "ok");
                    }
                    else if (error == "invalid_code")
                    {   
                        await Application.Current.MainPage
                            .DisplayAlertAsync("Error", "This email is unreal", "ok");
                    }
                    break;

                case "success":
                    packet.body.TryGetValue("error", out string sucess);
                    sucess = sucess?.Trim().ToLower();
                    if (sucess == "email_verified")
                    {
                        await Application.Current.MainPage
                            .DisplayAlertAsync("Успех", "Почта подтверждена", "OK");
                        // закрываем попап
                        await CloseAsync();
                        // переходим на главную
                        await Shell.Current.GoToAsync(nameof(ChangedPasswordPopup));
                    }
                    break;
            }
        });
    }



    private async void VerifButtonClicked(object sender, EventArgs e)
    {
        string verifCode = CodeEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(verifCode) || verifCode.Length < 9)
        {
            await Application.Current.MainPage.DisplayAlertAsync(
               "Ошибка", "Введите корректный код", "OK");
            return;
        }

        try
        {
            //string message = $"verify_data;{verifCode}";
            await _tcpService.SendJsonAsync("verify_data", new() { ["code"] = verifCode });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlertAsync("Ошибка", ex.Message, "OK");
        }
    }
    private void CodeEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isEditing) return;

        _isEditing = true;

        var entry = (Entry)sender;
        string text = entry.Text ?? "";

        // Убираем всё, кроме букв/цифр
        text = new string(text.Where(char.IsLetterOrDigit).ToArray());

        // Добавляем "-" после каждых 3 символов
        var formatted = new StringBuilder();
        for (int i = 0; i < text.Length; i++)
        {
            if (i > 0 && i % 3 == 0)
                formatted.Append('-');

            formatted.Append(text[i]);
        }

        entry.Text = formatted.ToString();

        // Ставим курсор в конец
        entry.CursorPosition = entry.Text.Length;

        _isEditing = false;
    }
}