// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Options;

namespace Maestro.Web.Ui.Auth;

public class TokenProvider(ILocalStorageService localStorage,
    IOptions<OidcProviderOptions> providerOptions,
    IAccessTokenProvider accessTokenProvider,
    AuthenticationStateProvider stateProvider)
{
    public async Task<string?> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        if (providerOptions.Value.Authority != "https://accounts.google.com/")
        {
            var tokenRequest = await accessTokenProvider.RequestAccessToken();
            if (tokenRequest.TryGetToken(out var token))
            {
                return token.Value;
            }

            return null;
        }
        await localStorage.GetItemAsStringAsync("", cancellationToken);

        return null;
    }
}
