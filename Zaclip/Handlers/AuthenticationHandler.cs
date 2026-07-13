using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Zaclip.States;

namespace Zaclip.Handlers
{
    public class AuthenticationHandler : DelegatingHandler
    {
        private readonly TokenStore _tokenStore;
        public AuthenticationHandler(TokenStore tokenStore)
        {
            _tokenStore = tokenStore;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
            )
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenStore.AccessToken);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
