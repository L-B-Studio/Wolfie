using CommunityToolkit.Maui.Extensions;
using Wolfie.Helpers;
using Wolfie.Services;

namespace Wolfie.Pages;

public partial class RegistrationPage : ContentPage
{
    private bool _isDarktheme = false;
    private readonly SslClientService _tcpService;
    //djfhjdhfdjhf
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

        string dataMessage = $"registration_data;{username};{email};{createPass};{birthday:yyyy-MM-dd}";

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(createPass) ||
            string.IsNullOrWhiteSpace(againPass))
        {
            await DisplayAlertAsync("Ошибка", "Все поля должны быть заполнены", "Ок");
            return;
        }

        if (!IsValidBirthdayDate(birthday))
        {
            await DisplayAlertAsync("Ошибка", "Укажите корректную дату рождения (от 3 до 120 лет)", "Ок");
            return;
        }

        if (createPass != againPass)
        {
            await DisplayAlertAsync("Ошибка", "Пароли не совпадают", "Ок");
            return;
        }

        if (createPass.Length < 6)
        {
            await DisplayAlertAsync("Ошибка", "Пароль должен содержать минимум 6 символов", "Ок");
            return;
        }

        if (username.Length < 3)
        {
            await DisplayAlertAsync("Ошибка", "Имя пользователя должно содержать минимум 3 символа", "Ок");
            return;
        }

        if (!IsValidEmail(email))
        {
            await DisplayAlertAsync("Ошибка", "Введите корректную почту", "Ок");
            return;
        }

        if (!AgreeCheckBox.IsChecked)
        {
            await DisplayAlertAsync("Ошибка", "Необходимо согласиться с правилами", "Ок");
            return;
        }

        try
        {
            await _tcpService.SendAsync(dataMessage);
        }
        catch
        {
            await DisplayAlertAsync("Ошибка", "Нет соединения с сервером", "Ок");
        }
    }

    private async void OnMessageReceived(string msg)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            switch (msg)
            {
                case "ERROR;REG_FAILED;EMAIL_EXISTS":
                    await DisplayAlertAsync("Ошибка", "Почта уже зарегистрирована", "Ок");
                    break;

                case "ERROR;REG_FAILED;USERNAME_EXISTS":
                    await DisplayAlertAsync("Ошибка", "Имя пользователя занято", "Ок");
                    break;

                default:
                    await DisplayAlertAsync("Успех", "Регистрация выполнена!", "Войти");
                    await Navigation.PushAsync(new LoginPage());
                    break;
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
            SubTitleLabel.TextColor = Colors.White;
            AgreeSpan.TextColor = Colors.White;
            AndSpan.TextColor = Colors.White;
            RegistrationButton.BackgroundColor = Colors.White;
            RegistrationButton.TextColor = Colors.Black;

            CreateAccountLabel.TextColor = Colors.White;
            NicknameLabel.TextColor = Colors.White;
            BirthdayLabel.TextColor = Colors.White;
            EmailLabel.TextColor = Colors.White;
            PassLabel.TextColor = Colors.White;
        }
        else
        {
            Application.Current.UserAppTheme = AppTheme.Light;
            ThemeButton.Source = "moon.png";
            BackButton.Source = "back_button.png";
            Logo.Source = "logo.png";
            MainLayout.BackgroundColor = Colors.White;

            TitleLabel.TextColor = Colors.Black;
            SubTitleLabel.TextColor = Colors.Black;
            AgreeSpan.TextColor = Colors.Black;
            AndSpan.TextColor = Colors.Black;
            RegistrationButton.BackgroundColor = Colors.Black;
            RegistrationButton.TextColor = Colors.White;

            CreateAccountLabel.TextColor = Colors.Black;
            NicknameLabel.TextColor = Colors.Black;
            BirthdayLabel.TextColor = Colors.Black;
            EmailLabel.TextColor = Colors.Black;
            PassLabel.TextColor = Colors.Black;
        }
    }

    //private async void OnTermsTapped(object sender, EventArgs e)
    //{
    //    var popup = new TermsPopup();
    //    await this.ShowPopupAsync(popup);
    //}

    //private async void OnPrivacyTapped(object sender, EventArgs e)
    //{
    //    var popup = new PrivacyPopup();
    //    await this.ShowPopupAsync(popup);
    //}

}