using System;
using System.Collections.Generic;
using System.Text;
using Zaclip.Dtos;

namespace Zaclip.Services.AuthService
{
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(
            string email,
            string password);
    }
}
