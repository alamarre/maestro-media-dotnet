using Blazored.LocalStorage;
using Maestro.Components.Shared.Utilities;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Options;

namespace Maestro.Web.Ui.Auth;

public class TokenProvider(ILocalStorageService localStorage,
    IOptions<OidcProviderOptions> providerOptions,
    IAccessTokenProvider accessTokenProvider,
    AuthenticationStateProvider stateProvider) : ITokenProvider
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
