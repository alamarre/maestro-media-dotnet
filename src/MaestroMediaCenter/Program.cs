using Maestro;
using Maestro.Controllers;
using Maestro.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer("self", options => {
           options.SecurityTokenValidators.Clear();
           options.SecurityTokenValidators.Add( new LocalSecurityTokenValidator());
        })
        .AddJwtBearer("external", options =>
        {
            /*options.Authority = "https://your-auth-server.com"; // Your auth server URL
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = "https://your-auth-server.com", // Your auth server URL
                ValidAudience = "your-api-audience", // Your API audience identifier
                ValidateLifetime = true
            };
            options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{options.Authority}/.well-known/jwks_uri", // Your custom JWKS endpoint
                new OpenIdConnectConfigurationRetriever()
            );*/
        });

builder.Services.AddAuthorization(options => {
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes("self", "external")
                .Build();

});

builder.Services.AddDbContext<PostgresDbContext>(ServiceLifetime.Transient);

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

builder.Services.AddSingleton(typeof(ITable<>),typeof(Table<>));
builder.Services.AddSingleton<IController, VideosController>();
builder.Services.AddSingleton<IController, PingController>();

var app = builder.Build();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();


foreach(var controller in app.Services.GetServices<IController>()) {
    controller.MapRoutes(app);
}

SampleController.MapRoutes(app);

app.Run();
