using System;
using System.Collections.Generic;
using System.Text;
using Zaclip.Dto;

namespace Zaclip.Service.Interface
{
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(
            string email,
            string password);
    }
}
