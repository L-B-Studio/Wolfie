using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Models.DTOObjects.Chats
{
    //access_token
    public record GetChatResponse
    {
        public string chat_uid { get; init; } = string.Empty;
        public string chat_name { get; init; } = string.Empty;
        public string chat_type { get; init; } = string.Empty;
        public string last_message_text { get; init; } = string.Empty;
        public string last_message_sender { get; init; } = string.Empty;
        public int? not_read_messages_countun { get; init; } = 0;
    }
}
