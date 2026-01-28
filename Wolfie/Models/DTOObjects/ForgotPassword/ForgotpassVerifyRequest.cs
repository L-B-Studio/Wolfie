using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Models.DTOObjects.ForgotPassword
{
    public record ForgotpassVerifyRequest
    {
        public string Email { get; init; } = string.Empty;
        public string Code { get; init; } = string.Empty;
    }
}
