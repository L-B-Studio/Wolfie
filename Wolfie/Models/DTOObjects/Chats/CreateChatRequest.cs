using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Models.DTOObjects.Chats
{
    public record CreateChatRequest
    {
        public string Chat_name { get; init; } = string.Empty;
        public string Chat_type { get; init; } = string.Empty;
        public List<string>? Member_uids { get; init; } = null;
    }
}
