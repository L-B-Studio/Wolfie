using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Models.DTOObjects.Chats
{
    public record CreateChatRequest
    {
        public string Chat_name { get; init; }
        public string Chat_type { get; init; }
        public List<string> Member_uids { get; init; }
    }
}
