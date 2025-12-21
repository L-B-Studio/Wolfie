
using SQLite;
using Wolfie.Models;

namespace Wolfie.Services;

public class LocalDbService
{
    private readonly SQLiteAsyncConnection _db;

    public LocalDbService()
    {
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "wolfie.db3");
        _db = new SQLiteAsyncConnection(dbPath);
        _db.CreateTableAsync<ChatItemModel>().Wait();
        _db.CreateTableAsync<UserItemModel>().Wait();
    }

    //================= User =================
    //public async void CreateUser(string username , string status , string avatar) 
    //{
    //    bool IsPrime = false;
    //    bool IsLogger = false;
    //    bool IsDeveloper = false;
    //    switch (status){ 
    //        case "prime":
    //            IsPrime = true;
    //            break;
    //        case "logger":
    //            IsLogger = true;
    //            break;
    //        case "developer":
    //            IsDeveloper = true;
    //            break;
    //
    //    }
    //    var newUser = new UserItemModel
    //    {
    //        Username = username ,
    //        ChatAvatarUri = avatar ,
    //        IsPrime = IsPrime,
    //        IsDeveloper = IsDeveloper,
    //        IsLogger = IsLogger
    //    };
    //    _db.InsertAsync(newUser).Wait();
    //}

    // ================= CHATS =================

    public Task<List<ChatItemModel>> GetAllChatsAsync() =>
        _db.Table<ChatItemModel>()
           .OrderByDescending(c => c.LastActivityDate)
           .ToListAsync();

    public Task SaveChatAsync(ChatItemModel chat) =>
        _db.InsertOrReplaceAsync(chat);

    public Task<ChatItemModel?> GetChatAsync(string chatId) =>
        _db.FindAsync<ChatItemModel>(chatId);

    // ================= MESSAGES =================

    private string ChatTable(string chatId) =>
        $"ChatMessages_{chatId.Replace("-", "_")}";

    public async Task CreateChatTableIfNotExistsAsync(string chatId)
    {
        var table = ChatTable(chatId);

        var sql = $@"
    CREATE TABLE IF NOT EXISTS {table} (
        MessageId TEXT PRIMARY KEY,
        Getter TEXT,
        Sender TEXT,
        Message TEXT,
        MessageType TEXT,
        MediaContentUri TEXT,
        Timestamp TEXT,
        DeliveryStatus INTEGER,
        isReaded INTEGER,
        IsDeleted INTEGER,
        IsEdited INTEGER,
        ReplyToMessageId TEXT
    )";

        await _db.ExecuteAsync(sql);
    }


    public async Task SaveMessageAsync(string chatId, MessageItemModel msg)
    {
        await CreateChatTableIfNotExistsAsync(chatId);

        var table = ChatTable(chatId);

        var sql = $@"
    INSERT OR REPLACE INTO {table}
    (MessageId, Getter, Sender, Message, MessageType, MediaContentUri, Timestamp, DeliveryStatus, isReaded, IsDeleted, IsEdited, ReplyToMessageId)
    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

        await _db.ExecuteAsync(sql,
            msg.MessageId,
            msg.Getter,
            msg.Sender,
            msg.Message,
            msg.MessageType,
            msg.MediaContentUri,
            msg.Timestamp.ToString("o"),
            msg.DeliveryStatus,
            msg.isReaded ? 1 : 0,
            msg.IsDeleted ? 1 : 0,
            msg.IsEdited ? 1 : 0,
            msg.ReplyToMessageId ?? "");
    }



    public async Task<List<MessageItemModel>> GetMessagesAsync(string chatId)
    {
        await CreateChatTableIfNotExistsAsync(chatId);

        var table = ChatTable(chatId);

        var list = await _db.QueryAsync<MessageItemModel>($"SELECT * FROM {table} ORDER BY Timestamp");

        // Конвертируем INT → bool для полей isReaded, IsDeleted, IsEdited
        foreach (var msg in list)
        {
            // SQLite-net автоматически преобразует INTEGER в bool, если свойства bool, так что можно пропустить
        }

        return list;
    }


    public async Task LogicalDeleteMessageAsync(string chatId, string messageId)
    {
        var table = ChatTable(chatId);
        await _db.ExecuteAsync(
            $"UPDATE {table} SET IsDeleted = 1 WHERE MessageId = ?",
            messageId);
    }

    public async Task UpdateMessageStatusAsync(string chatId, string messageId, int status)
    {
        var table = ChatTable(chatId);
        await _db.ExecuteAsync(
            $"UPDATE {table} SET DeliveryStatus = ? WHERE MessageId = ?",
            status, messageId);
    }
}
