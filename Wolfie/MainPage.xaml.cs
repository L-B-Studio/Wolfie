using Wolfie.Helpers;
using Wolfie.Pages;
using Wolfie.Services;

namespace Wolfie
{
    public partial class MainPage : ContentPage
    {
        private readonly SslClientService _client;

        public MainPage()
        {
            InitializeComponent();
            _client = SslClientHelper.GetService<SslClientService>();
            _ = StartConnection();
        }

        private async Task StartConnection()
        {
            try
            {
                await _client.EnsureConnectedAsync();

                await Shell.Current.GoToAsync(nameof(LoginPage));
            }
            catch(Exception ex)
            {
                await DisplayAlertAsync("Error" , ex.Message , "ok");

                _ = StartConnection();
            }
        }
    }
}
