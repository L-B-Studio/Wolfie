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

        //    string html = @"
        //<iframe width='100%' height='100%'
        //        src='https://youtu.be/gXKQWIgq-9w?si=Tk4NznnNUGRpSFl2'
        //        frameborder='0'
        //        allow='autoplay'>
        //</iframe>";
        //
        //    VideoWebView.Source = new HtmlWebViewSource
        //    {
        //        Html = html
        //    };
        
        }

        private async Task StartConnection()
        {
            try
            {
                await _client.EnsureConnectedAsync();

                await Shell.Current.GoToAsync(nameof(LoginPage));
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", ex.Message, "ok");

                _ = StartConnection();
            }
        }

        //private void VideoPlayer_MediaEnded(object sender, EventArgs e)
        //{
        //    BackgroundVideo.Play();
        //}

    }
}
