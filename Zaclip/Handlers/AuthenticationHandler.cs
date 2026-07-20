using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Zaclip.Services.AuthService;
using Zaclip.States;

namespace Zaclip.Handlers
{
    public class AuthenticationHandler : DelegatingHandler
    {
        private readonly TokenStore _tokenStore;
        private readonly IAuthService _authService;
        public AuthenticationHandler(TokenStore tokenStore, IAuthService authService)
        {
            _tokenStore = tokenStore;
            _authService = authService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
            )
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenStore.AccessToken);
            var result = await base.SendAsync(request, cancellationToken);
            if(result.StatusCode == System.Net.HttpStatusCode.Unauthorized && _tokenStore.RefreshToken != null)
            {
                var refreshResult = await _authService.RefreshAsync(_tokenStore.RefreshToken);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenStore.AccessToken);
                result = await base.SendAsync(request, cancellationToken);
            }
            return result;
        }
    }
}
