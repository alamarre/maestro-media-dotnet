using Maestro;
using Maestro.Auth;
using Maestro.Controllers;
using Maestro.Database;
using Maestro.Services;
using Maestro.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT withoute the Bearer prefix into field",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme
            {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
            },
            new string[] { }
        }
    });
    options.AddServer(new OpenApiServer
    {
        Url = "/"
    });
    options.AddServer(new OpenApiServer
    {
        Url = "https://api.videos.omny.ca/"
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

builder.Services.AddDbContextFactory<MediaDbContext>(options => {
});

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

builder.Services.AddSingleton(typeof(ITable<>),typeof(Table<>));
builder.Services.AddSingleton<LocalSecurityTokenValidator>();
builder.Services.AddSingleton<ICacheService, DbCacheSerice>();

builder.Services.AddSingleton<VideoService>();
builder.Services.AddSingleton<VideoUtilities>();

builder.Services.AddSingleton<IController, VideosController>();
builder.Services.AddSingleton<IController, PingController>();
builder.Services.AddSingleton<IController, LocalAuthController>();
builder.Services.AddSingleton<IController, VideoSourcesController>();
builder.Services.AddSingleton<IController, ServersController>();

UserContextSetter setter = new UserContextSetter();
builder.Services.AddSingleton<IUserContextSetter>(setter);
builder.Services.AddSingleton<IUserContextProvider>(setter);

var app = builder.Build();
app.UseCors();

if( app.Environment.IsDevelopment() ) {
    app.UseSwagger();
    app.UseSwaggerUI( options => {
        options.EnablePersistAuthorization();
    } );
}

app.UseAuthentication();
app.UseAuthorization();
app.UseUserContextProvider();

foreach(var controller in app.Services.GetServices<IController>()) {
    controller.MapRoutes(app);
}

SampleController.MapRoutes(app);

app.Run();

public partial class Program { }
