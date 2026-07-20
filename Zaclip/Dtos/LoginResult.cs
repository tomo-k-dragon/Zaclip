using System;
using System.Collections.Generic;
using System.Text;

namespace Zaclip.Dtos
{
    public class LoginResult
    {
        public LoginResult(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
        public string AccessToken { get; set; } = "";
        public string RefreshToken { get; set; } = "";
    }
}
