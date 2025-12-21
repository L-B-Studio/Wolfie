using System.Text.Json;
using Wolfie.Helpers;
using Wolfie.Models;
using Wolfie.Services;

public class ChatStorageService
{
    private readonly LocalDbService _db;
    private readonly SslClientService _client;

    public event Action<ChatItemModel>? ChatAdded;
    public event Action<ChatItemModel>? ChatUpdated;

    public ChatStorageService()
    {
        _db = ServiceClientHelper.GetService<LocalDbService>()!;
        _client = ServiceClientHelper.GetService<SslClientService>()!;

        _client.MessageReceived += OnServerMessage;
        _ = CreateSavedMessagesChatAsync();
    }

    private async Task CreateSavedMessagesChatAsync()
    {
        const string savedChatId = "saved_messages"; // уникальный ID
        var existing = await _db.GetChatAsync(savedChatId);
        if (existing != null)
            return;

        var chat = new ChatItemModel
        {
            ChatId = savedChatId,
            ChatTitle = "Saved Messages",
            ChatAvatarName = "default_user_icon.png", // локальный ресурс аватара
            LastMessage = string.Empty,
            LastActivityDate = DateTime.UtcNow,
            IsUnreaded = false,
            UnreadedMessageCount = 0
        };

        await _db.SaveChatAsync(chat);
        ChatAdded?.Invoke(chat);
    }

    public async Task<List<ChatItemModel>> GetAllChatsAsync()
    {
        return await _db.GetAllChatsAsync();
    }


    // ====== SERVER EVENTS ======

    private void OnServerMessage(string raw)
    {
        var pkg = JsonSerializer.Deserialize<ServerJsonPackage>(raw);
        if (pkg == null) return;

        switch (pkg.header)
        {
            case "chat_created":
                HandleChatCreated(pkg);
                break;

            case "chat_updated":
                HandleChatUpdated(pkg);
                break;

            case "new_message":
                HandleNewMessage(pkg);
                break;
        }
    }

    private async void HandleChatCreated(ServerJsonPackage pkg)
    {
        var chat = ParseChat(pkg);
        await _db.SaveChatAsync(chat);
        ChatAdded?.Invoke(chat);
    }

    private async void HandleChatUpdated(ServerJsonPackage pkg)
    {
        var chat = ParseChat(pkg);
        await _db.SaveChatAsync(chat);
        ChatUpdated?.Invoke(chat);
    }

    private async void HandleNewMessage(ServerJsonPackage pkg)
    {
        var chatId = pkg.body["ChatId"];
        var text = pkg.body["Message"];

        var chat = await _db.GetChatAsync(chatId);
        if (chat == null) return;

        chat.LastMessage = text;
        chat.LastActivityDate = DateTime.UtcNow;
        chat.IsUnreaded = true;
        chat.UnreadedMessageCount++;

        await _db.SaveChatAsync(chat);
        ChatUpdated?.Invoke(chat);
    }

    private ChatItemModel ParseChat(ServerJsonPackage pkg)
    {
        return new ChatItemModel
        {
            ChatId = pkg.body["ChatId"],
            ChatTitle = pkg.body["Title"],
            LastMessage = pkg.body.GetValueOrDefault("LastMessage", ""),
            LastActivityDate = DateTime.UtcNow
        };
    }
}
