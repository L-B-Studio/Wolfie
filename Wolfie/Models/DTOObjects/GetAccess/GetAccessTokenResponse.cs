using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Models.DTOObjects.GetAccess
{
    public record GetAccessTokenResponse
    {
        public string token_refresh { get; init; } = string.Empty;
        public string token_access { get; init; } = string.Empty; 
    }
}
