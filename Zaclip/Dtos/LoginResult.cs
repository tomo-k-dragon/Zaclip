using System;
using System.Collections.Generic;
using System.Text;

namespace Zaclip.Dtos
{
    public class LoginResult
    {
        public LoginResult(bool isSuccess, string? accessToken = null, string? refreshToken = null, string? errorMessage = null)
        {
            IsSuccess = isSuccess;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ErrorMessage = errorMessage;
        }
        public bool IsSuccess { get; set; } = false;
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
