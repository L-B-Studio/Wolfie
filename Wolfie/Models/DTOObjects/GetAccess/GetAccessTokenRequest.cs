using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Models.DTOObjects.GetAccess
{
    public record GetAccessTokenRequest
    {
        public string Token_refresh { get; init; }
        public string? Devic_name { get; init; } = null;
        public string? Device_type { get; init; } = null;
        public string? Place_auth { get; init; } = null;

    }
}
