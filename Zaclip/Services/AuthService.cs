using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using Zaclip.Dto;
using Zaclip.Service.Interface;
using Zaclip.Settings;
using Zaclip.States;

namespace Zaclip.Service
{
    public class AuthService: IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly TokenStore _tokenStore;
        private readonly SessionContext _session;

        public AuthService(HttpClient httpClient, IOptions<ApiSettings> options, TokenStore tokenStore, SessionContext session)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
            _tokenStore = tokenStore;
            _session = session;
        }

        public async Task<LoginResult> LoginAsync(string email, string password)
        {
            var request = new
            {
                Email = email,
                Password = password
            };

            var response =
                await _httpClient.PostAsJsonAsync(
                    "http://localhost:60262/api/auth/login",
                    request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception();
            }

            var token = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if(token == null)
            {
                throw new Exception();
            }
            _tokenStore.Set(token.Token, token.RefreshToken, DateTime.Now.AddSeconds(token.ExpiresIn));
            _session.Login(email);

            return new LoginResult(token.Token, token.RefreshToken);
        }
    }
}