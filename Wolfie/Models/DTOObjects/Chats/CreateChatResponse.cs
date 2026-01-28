using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Models.DTOObjects.Chats
{
    public record CreateChatResponse
    {
        public string chat_uid { get; init; } = string.Empty;

        public byte[]? chat_avatar { get; init; } = null;
    }
}
