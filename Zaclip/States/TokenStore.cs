using System;
using System.Collections.Generic;
using System.Text;

namespace Zaclip.States
{
    public class TokenStore
    {
        public string? AccessToken { get; private set; }
        public string? RefreshToken { get; private set; }
        public DateTime ExpiresAt { get; private set; }

        public void Set(string accessToken, string refreshToken, DateTime exipiresAt)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresAt = ExpiresAt;
        }

        public void Clear()
        {
            AccessToken = null;
            RefreshToken = null;
            ExpiresAt = DateTime.MinValue;
        }
    }
}
