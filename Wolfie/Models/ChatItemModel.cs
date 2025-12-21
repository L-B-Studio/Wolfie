using SQLite;

namespace Wolfie.Models;

[Table("Chats")]
public class ChatItemModel
{
    [PrimaryKey]
    public string ChatId { get; set; } = string.Empty;

    public string ChatTitle { get; set; } = string.Empty;

    public byte[]? ChatAvatarBytes { get; set; }
    public string? ChatAvatarName { get; set; }

    public string LastMessage { get; set; } = string.Empty;
    public DateTime LastActivityDate { get; set; }

    public bool IsPinned { get; set; }
    public bool IsUnreaded { get; set; }
    public int UnreadedMessageCount { get; set; }
}
