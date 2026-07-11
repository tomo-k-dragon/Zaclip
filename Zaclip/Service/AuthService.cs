using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using Zaclip.Dto;
using Zaclip.Service.Interface;
using Zaclip.Settings;

namespace Zaclip.Service
{
    public class AuthService: IAuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient, IOptions<ApiSettings> options)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
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

            return new LoginResult(token.Token, token.RefreshToken);
        }
    }
}