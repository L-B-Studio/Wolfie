using SQLite;
using System;

namespace Wolfie.Models
{
    // Model representing a message item in the database
    public class MessageItemModel
    {
        [PrimaryKey]
        public string MessageId { get; set; } = string.Empty;

        public string Getter { get; set; } = string.Empty;  // ChatId
        public string Sender { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
        public string MessageType { get; set; } = "Text";
        public string MediaContentUri { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        public int DeliveryStatus { get; set; } = 0;
        public bool isReaded { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsEdited { get; set; }

        public string ReplyToMessageId { get; set; }
    }
}
