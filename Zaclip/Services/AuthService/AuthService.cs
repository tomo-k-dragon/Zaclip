using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            var request = new { Email = email, Password = password };
            var response =
                await _httpClient.PostAsJsonAsync(
                    "/api/auth/login",
                    request);

            if (!response.IsSuccessStatusCode)
                throw new Exception();

            var token = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if(token == null) throw new Exception();

            await CompleteLoginAsync(email, token.Token, token.RefreshToken, DateTime.Now.AddSeconds(token.ExpiresIn));
            return new LoginResult(token.Token, token.RefreshToken);
        }

        public async Task<LoginResult> RefreshAsync(string email, string refreshToken)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", new { refreshToken = refreshToken });

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException("Refresh token is invalid or expired.");
            else if (!response.IsSuccessStatusCode)
                    throw new Exception($"Refresh failed. StatusCode={response.StatusCode}");

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (loginResponse == null)
                throw new Exception("Response body is empty.");

            var token = loginResponse.Token;
            var newRefreshToken = loginResponse.RefreshToken;
            await CompleteLoginAsync(email, token, newRefreshToken, DateTime.Now.AddSeconds(loginResponse.ExpiresIn));
            return new LoginResult(token, newRefreshToken);
        }

        public async Task AutoLoginAsync()
        {
            try
            {
                var credential = await _credentialService.LoadAsync();
                var loginResult = await RefreshAsync(credential.Email, credential.RefreshToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Auto login failed: {ex.Message}");
            }
        }

        // <summary>ログイン情報の保存を行う</summary>
        private async Task CompleteLoginAsync(string email, string token, string refreshToken, DateTime expiresAt)
        {
            _tokenStore.Set(token, refreshToken, expiresAt);
            _session.Login(email);
            await _credentialService.SaveAsync(email, refreshToken);
        }
    }
}