using Wolfie.Helpers;
using Wolfie.Models;
using Wolfie.Services;
using Wolfie.ViewModels;

namespace Wolfie.Pages;

public partial class ChatListPage : ContentPage
{
    private readonly SslClientService _client;
    public ChatListPage(ChatPageViewModel viewModel)
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        NavigationPage.SetBackButtonTitle(this, null);
        _client = SslClientHelper.GetService<SslClientService>();
        BindingContext = viewModel;
    }

    private async void ChatList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        await DisplayAlertAsync("TAPPED", "U TAPPED TO CHAT", "ok");
        //var current = e.CurrentSelection.FirstOrDefault() as ChatItem;
        //if (current == null)
        //    return;

        // Снимаем выделение, чтобы можно было нажимать повторно
        //((CollectionView)sender).SelectedItem = null;

        // Навигация в ChatPage, передавая ID чата
        //await Navigation.PushAsync(new ChatPage(current.ChatId, current.ChatTitle));
    }


}