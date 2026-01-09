
using System;
using System.Collections.Generic;
using System.Text;
using Wolfie.Helpers;
using Wolfie.Services;

namespace Wolfie.Auth
{
    // Class representing the authentication state
    public class AuthState
    {
        public string? AccessToken { get; set; }
        public bool IsAuthorized => AccessToken != null;
    }
}
