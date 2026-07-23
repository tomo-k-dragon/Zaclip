using System;
using System.Collections.Generic;
using System.Text;
using Zaclip.Dtos;

namespace Zaclip.Services.Credential
{
    public interface ICredentialService
    {
        public Task SaveAsync(string email, string refreshToken);
        public Task<CredentialData> LoadAsync();
        public Task DeleteAsync();
    }
}
