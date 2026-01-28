using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Models.DTOObjects.Chats
{
    public record GetChatRequest
    {
        public string token_access { get; init; } = string.Empty;
       
    }
}
