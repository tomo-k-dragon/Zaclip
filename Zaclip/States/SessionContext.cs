using System;
using System.Collections.Generic;
using System.Text;

namespace Zaclip.States
{
    public class SessionContext
    {
        public bool IsLoggedIn { get; private set; } = false;
        public string? UserEmail { get; private set; }
        public event Action? SessionChanged;

        public void Login(string email)
        {
            IsLoggedIn = true;
            UserEmail = email;
            SessionChanged?.Invoke();
        }

        public void Logout()
        {
            IsLoggedIn = false;
            UserEmail = null;
            SessionChanged?.Invoke();
        }
    }
}
