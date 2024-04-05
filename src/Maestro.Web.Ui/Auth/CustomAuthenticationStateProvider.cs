using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Maestro.Web.Ui.Auth;

public class CustomAuthenticationStateProvider(
    IServiceProvider serviceCollection)
    : AuthenticationStateProvider
{

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var defaultProvider = serviceCollection.GetServices<AuthenticationStateProvider>().First(a => a != this);

        var userInfo = await defaultProvider.GetAuthenticationStateAsync();
        
        string? jwtToken = null;

        if (string.IsNullOrEmpty(jwtToken))
        {
            //var userDataKey = $"oidc.user:{NavigationManager.BaseUri}:{clientId}";
            //var userData = await JSRuntime.InvokeAsync<UserData>("sessionStorage.getItem", userDataKey);
            
            // No JWT found, possibly exchange Google ID token for JWT
            // This assumes you have a way to get the Google ID token here, which may require different logic
        }

        var identity = new ClaimsIdentity();
        if (!string.IsNullOrEmpty(jwtToken))
        {
            // Assume the JWT is valid and contains claims
            identity = new ClaimsIdentity(ParseClaimsFromJwt(jwtToken), "jwtAuthType");
        }

        var user = new ClaimsPrincipal(identity);
        return userInfo;
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        // Implement parsing JWT to extract claims
        return new List<Claim>();
    }
}

