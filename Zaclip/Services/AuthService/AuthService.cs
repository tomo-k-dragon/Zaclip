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
            try {
                var request = new { Email = email, Password = password };
                var response =
                    await _httpClient.PostAsJsonAsync(
                        "/api/auth/login",
                        request);

                if (!response.IsSuccessStatusCode)
                    return new LoginResult(false, errorMessage: "ログインに失敗しました。");

                var token = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (token == null) return new LoginResult(false, errorMessage: "ログインに失敗しました。");

                await CompleteLoginAsync(email, token.Token, token.RefreshToken, DateTime.Now.AddSeconds(token.ExpiresIn));
                return new LoginResult(true, token.Token, token.RefreshToken);
            } catch (Exception ex)
            {
                return new LoginResult(false, errorMessage: "ログインに失敗しました。");
            }
        }

        public async Task<LoginResult> RefreshAsync(string email, string refreshToken)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", new { refreshToken = refreshToken });
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return new LoginResult(false, errorMessage: "ログインに失敗しました。");
                else if (!response.IsSuccessStatusCode)
                    return new LoginResult(false, errorMessage: "ログインに失敗しました。");

                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (loginResponse == null)
                    return new LoginResult(false, errorMessage: "ログインに失敗しました。");

                var token = loginResponse.Token;
                var newRefreshToken = loginResponse.RefreshToken;
                await CompleteLoginAsync(email, token, newRefreshToken, DateTime.Now.AddSeconds(loginResponse.ExpiresIn));
                return new LoginResult(true, token, newRefreshToken);
            }
            catch (Exception ex)
            {
                return new LoginResult(false, errorMessage: "ログインに失敗しました。");
            }
        }

        public async Task AutoLoginAsync()
        {
            var credential = await _credentialService.LoadAsync();
            if (credential == null) return;

            var loginResult = await RefreshAsync(credential.Email, credential.RefreshToken);
        }

        public async Task LogoutAsync(string refreshToken)
        {
            if (!_session.IsLoggedIn)
                return;

            try
            {
                var result = await _httpClient.PostAsJsonAsync("api/auth/logout", new { refreshToken = refreshToken });
                if (!result.IsSuccessStatusCode)
                    throw new Exception("ログアウトに失敗しました。");

                _tokenStore.Clear();
                _session.Logout();
                await _credentialService.DeleteAsync();
            }
            catch (Exception ex)
            {
                throw;
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