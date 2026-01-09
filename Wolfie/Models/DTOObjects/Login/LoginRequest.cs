using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Models.DTOObjects.Login
{
    public record LoginRequest
    {
        public string Email { get; init; }
        public string Password { get; init; }
        public string? Device_id { get; init; } = null;
        public string? Device_type { get; init; } = null;
        public string? Place_authorization { get; init; } = null;
    }
}
