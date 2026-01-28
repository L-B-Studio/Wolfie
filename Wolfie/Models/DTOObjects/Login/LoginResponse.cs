using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Wolfie.Models.DTOObjects.Login
{
    public record LoginResponse
    {
        public string token_refresh { get; init; } = string.Empty;
        public string token_access { get; init; } = string.Empty;
    }
}
