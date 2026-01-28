using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Models.DTOObjects.Message
{
    public record GetMessagesRequest
    {
        public string token_access { get; init; } = string.Empty;
    }
}
