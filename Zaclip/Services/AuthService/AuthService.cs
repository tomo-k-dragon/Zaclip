using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using Zaclip.Dtos;
using Zaclip.Services.Credential;
using Zaclip.Settings;
using Zaclip.States;

namespace Zaclip.Services.AuthService
{
    public class AuthService: IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly TokenStore _tokenStore;
        private readonly SessionContext _session;
        private readonly ICredentialService _credentialService;

        public AuthService(HttpClient httpClient, IOptions<ApiSettings> options, TokenStore tokenStore, SessionContext session, ICredentialService credentialService)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
            _tokenStore = tokenStore;
            _session = session;
            _credentialService = credentialService;
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
                    "/api/auth/login",
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
            await _credentialService.SaveAsync(email, token.RefreshToken);

            return new LoginResult(token.Token, token.RefreshToken);
        }

        public async Task<LoginResult> RefreshAsync(string refreshToken)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", new { refreshToken = refreshToken });

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException("Refresh token is invalid or expired.");
            else if (!response.IsSuccessStatusCode)
                    throw new Exception($"Refresh failed. StatusCode={response.StatusCode}");

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (loginResponse == null)
                throw new Exception("Response body is empty.");

            _tokenStore.Set(loginResponse.Token, loginResponse.RefreshToken, DateTime.Now.AddSeconds(loginResponse.ExpiresIn));
            return new LoginResult(loginResponse.Token, loginResponse.RefreshToken);
        }

        public async Task AutoLoginAsync()
        {
            try
            {
                var credential = await _credentialService.LoadAsync();
                var loginResult = await RefreshAsync(credential.RefreshToken);
                _session.Login(credential.Email);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Auto login failed: {ex.Message}");
            }
        }
    }
}