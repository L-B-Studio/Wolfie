using Microsoft.Maui.Controls;
using Wolfie.Helpers;
using Wolfie.Services;
using Wolfie.ViewModels;

namespace Wolfie.Pages;

public partial class ChatListPage : ContentPage
{
    private readonly ChatItemViewModel _viewModel;

    public ChatListPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        _viewModel = new ChatItemViewModel();
        BindingContext = _viewModel;
    }

    // Event handler for chat selection
    private async void ChatList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        //ChatSelected.Setter.BackgroudColor = Color.FromArgb("#4c5e75");
        if (e.CurrentSelection.Count == 0)
            return;

        var selectedChat = e.CurrentSelection[0] as ChatItemViewModel;
        if (selectedChat != null)
        {
            await DisplayAlertAsync("Chat is selected", $"You selected: {selectedChat.ChatTitle}", "ok");
        }

        ((CollectionView)sender).SelectedItem = null;
    }
}
