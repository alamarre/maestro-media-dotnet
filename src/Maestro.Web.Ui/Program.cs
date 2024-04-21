using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Maestro.Web.Ui;
using Maestro.Web.Ui.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using AuthorizationMessageHandler = Maestro.Web.Ui.Auth.AuthorizationMessageHandler;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<AuthorizationMessageHandler>();
var apiUrl = builder.Configuration.GetValue<string>("ApiUrl")!;
builder.Services.AddHttpClient("AuthenticatedClient", client => client.BaseAddress = new Uri(apiUrl))
    .AddHttpMessageHandler<AuthorizationMessageHandler>();

builder.Services.AddTransient(sp => 
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("AuthenticatedClient"));

//builder.Services.Configure<OidcProviderOptions>("Oidc");
builder.Services.AddOidcAuthentication(options =>
{
    // Configure your authentication provider options here.
    // For more information, see https://aka.ms/blazor-standalone-auth
    builder.Configuration.Bind("Oidc", options.ProviderOptions);

});

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

builder.Services.AddBlazoredLocalStorage();

var host = builder.Build();

await host.RunAsync();
