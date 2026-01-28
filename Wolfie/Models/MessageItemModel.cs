using System;

namespace Wolfie.Models
{
    public class MessageItemModel
    {
        public string ChatUid { get; set; } = string.Empty;
        public string MessageId { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string MessageText { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;

        // Свойство для определения, наше ли это сообщение (для UI)
        public bool IsOutgoing { get; set; }
    }
}
