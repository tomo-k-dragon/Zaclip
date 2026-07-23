using CredentialManagement;
using System;
using System.Collections.Generic;
using System.Text;
using Zaclip.Dtos;

namespace Zaclip.Services.Credential
{
    public class CredentialService
    {
        private const string Target = "Zaclip";
        public Task SaveAsync(string email, string refreshToken)
        {
            var credential = new CredentialManagement.Credential
            {
                Target = Target,
                Username = email,
                Password = refreshToken,
                PersistanceType = PersistanceType.LocalComputer,
                Type = CredentialType.Generic
            };
            if (!credential.Save())
                throw new Exception("資格情報の保存に失敗しました。");

            return Task.CompletedTask;
        }

        public Task<CredentialData> LoadAsync()
        {
            var credential = new CredentialManagement.Credential { Target = Target };
            if (!credential.Load())
                throw new Exception("資格情報の読み込みに失敗しました。");
            return Task.FromResult(new CredentialData
            {
                Email = credential.Username,
                RefreshToken = credential.Password
            });
        }

        public Task DeleteAsync()
        {
            var credential = new CredentialManagement.Credential { Target = Target };
            if (!credential.Delete())
                throw new Exception("資格情報の削除に失敗しました。");
            return Task.CompletedTask;
        }
    }
}
