using System;
using System.Collections.Generic;
using System.Text;

namespace Zaclip.Dtos
{
    public class CredentialData
    {
        public string Email { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
