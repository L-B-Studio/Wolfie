using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Models.DTOObjects.Message
{
    
    public record GetMessagesResponse
    {
        public string chat_uid { get; init; } = string.Empty;
        public string message_id { get; init; } = string.Empty;
        public string sender_id { get; init; } = string.Empty;
        public string message_text { get; init; } = string.Empty;
        public string created_at { get; init; } = string.Empty;
    }
}
