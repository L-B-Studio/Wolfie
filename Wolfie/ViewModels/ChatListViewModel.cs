using System.Collections.ObjectModel;
using Wolfie.Helpers;
using Wolfie.Models;
using Wolfie.Services;

namespace Wolfie.ViewModels;

public class ChatListViewModel
{
    private readonly ChatStorageService _storage;

    public ObservableCollection<ChatItemViewModel> Chats { get; } = new();

    public ChatListViewModel()
    {
        _storage = ServiceClientHelper.GetService<ChatStorageService>()!;

        // Загружаем чаты из БД
        LoadChatsAsync();

        // Подписываемся на события сервера
        _storage.ChatAdded += OnChatAdded;
        _storage.ChatUpdated += OnChatUpdated;
    }

    private async void LoadChatsAsync()
    {
        var allChats = await _storage.GetAllChatsAsync();
        Chats.Clear();
        foreach (var chat in allChats)
        {
            Chats.Add(new ChatItemViewModel(chat));
        }
    }

    private void OnChatAdded(ChatItemModel chat)
    {
        var vm = new ChatItemViewModel(chat);
        if (!Chats.Any(c => c.ChatTitle == chat.ChatTitle))
            Chats.Insert(0, vm); // добавляем сверху
    }

    private void OnChatUpdated(ChatItemModel chat)
    {
        var existing = Chats.FirstOrDefault(c => c.ChatId == chat.ChatId);
        if (existing != null) existing.UpdateFromModel(chat); // <-- теперь работает
    }

}
