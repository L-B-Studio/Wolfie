using CommunityToolkit.Maui.Views;
using Wolfie.Helpers;
using Wolfie.Services;

namespace Wolfie.Popups;

public partial class ChangedPasswordPopup : Popup
{
	private readonly SslClientService _client;
	public ChangedPasswordPopup()
	{
		InitializeComponent();
		_client = SslClientHelper.GetService<SslClientService>();
	}

	private async void OnConfirmClicked(object sender , EventArgs e)
	{
		string newPass = NewPasswordEntry.Text?.Trim();
		string repeatPass = RepeatPasswordEntry.Text?.Trim();

		if (string.IsNullOrWhiteSpace(newPass) || string.IsNullOrWhiteSpace(repeatPass))
		{
            await ShowAlert("Error", "All args isn't filled");
        }

		if (newPass != repeatPass) {
			await ShowAlert("Error", "Passwords must be одинаковые");
		}

		try
		{
			await _client.SendJsonAsync("changedpass_data;" , new{ password = newPass });
			await ShowAlert("SUCCESS" , "Try to login with new password now");
			await CloseAsync();
		}
		catch (Exception ex) { await ShowAlert("Error", ex.Message); }
    }

    private async Task ShowAlert(string title, string message)
    {
        await Application.Current.MainPage.DisplayAlertAsync(title, message, "OK");
    }
}