using System;
using System.Collections.Generic;
using System.Text;

namespace Zaclip.Dtos
{
    internal class LoginResponse
    {
        public string Token { get; init; } = string.Empty;

        public string RefreshToken { get; init; } = string.Empty;

        public int ExpiresIn { get; init; }
    }
}
