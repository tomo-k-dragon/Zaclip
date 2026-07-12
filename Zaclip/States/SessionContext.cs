using System;
using System.Collections.Generic;
using System.Text;

namespace Zaclip.States
{
    public class SessionContext
    {
        public bool IsLoggedIn { get; private set; } = false;
        public string? UserEmail { get; private set; }

        public void Login(string email)
        {
            IsLoggedIn = true;
            UserEmail = email;
        }

        public void Logout()
        {
            IsLoggedIn = false;
            UserEmail = null;
        }
    }
}
