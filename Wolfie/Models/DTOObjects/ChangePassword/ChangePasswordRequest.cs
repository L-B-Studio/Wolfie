using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Models.DTOObjects.ChangePassword
{
    public record ChangePasswordRequest
    {
        public string Token_reset { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string? Device_id { get; init; } = null;
        public string? Device_type { get; init; } = null;
        public string? Place_auth { get; init; } = null;
    }
}
