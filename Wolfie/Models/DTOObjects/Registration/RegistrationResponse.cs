using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Models.DTOObjects.Registration
{
    public record RegistrationResponse
    {
        public string token_refresh { get; init; }
        public string token_access { get; init; }
    }
}
