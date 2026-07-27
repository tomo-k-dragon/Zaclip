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
        private readonly SessionContext _sessionContext;
        private readonly IAuthService _authService;
        public AuthenticationHandler(TokenStore tokenStore, SessionContext sessionContext, IAuthService authService)
        {
            _tokenStore = tokenStore;
            _sessionContext = sessionContext;
            _authService = authService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
            )
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenStore.AccessToken);
            var result = await base.SendAsync(request, cancellationToken);
            if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized && _sessionContext.UserEmail != null && _tokenStore.RefreshToken != null)
            {
                var refreshResult = await _authService.RefreshAsync(_sessionContext.UserEmail, _tokenStore.RefreshToken);
                if (!refreshResult.IsSuccess)
                {
                    _tokenStore.Clear();
                    _sessionContext.Logout();
                    return result;
                }
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenStore.AccessToken);
                result = await base.SendAsync(request, cancellationToken);
            }
            return result;
        }
    }
}
