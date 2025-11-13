using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Models
{
    public class ChatItem
    {
        public string ChatId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
    }
}
