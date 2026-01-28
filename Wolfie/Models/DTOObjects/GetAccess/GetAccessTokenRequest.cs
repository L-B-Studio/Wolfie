using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Models.DTOObjects.GetAccess
{
    public record GetAccessTokenRequest
    {
        public string? token_refresh { get; init; } = null;
        public string? Device_id { get; init; } = null;
        public string? Device_type { get; init; } = null;
        public string? Place_auth { get; init; } = null;

    }
}
