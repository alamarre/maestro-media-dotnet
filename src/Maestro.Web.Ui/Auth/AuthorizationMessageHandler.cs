﻿using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Maestro.Web.Ui.Auth;

public class AuthorizationMessageHandler(IAccessTokenProvider accessTokenProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var tokenResult = await accessTokenProvider.RequestAccessToken();

        if (tokenResult.TryGetToken(out var token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

