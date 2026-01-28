using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Models.DTOObjects.ForgotPassword
{
    public record ForgotpassVerifyResponse
    {
        public string token_reset { get; init; } = string.Empty;
    }
}
