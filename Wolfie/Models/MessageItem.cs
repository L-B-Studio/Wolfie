using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Models
{
    public class MessageItem
    {
        public string ChatId { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime MessageTime { get; set; } = DateTime.Now;
    }
}
