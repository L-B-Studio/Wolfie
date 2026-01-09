using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Models.DTOObjects.ChangePassword
{
    public record ChangePasswordRequest
    {
        public string Token_reset { get; init; }
        public string Password { get; init; }
        public string? Device_id { get; init; } = null;
        public string? Device_type { get; init; } = null;
        public string? Place_auth { get; init; } = null;
    }
}
