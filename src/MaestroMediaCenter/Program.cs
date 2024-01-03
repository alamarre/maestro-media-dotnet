using Amazon.SQS;
using Maestro;
using Maestro.Auth;
using Maestro.Controllers;
using Maestro.Entities;
using Maestro.Events;
using Maestro.Events.Handlers;
using Maestro.Options;
using Maestro.Services;
using Maestro.Services.Background;
using Maestro.Utilities;
using MaestroMediaCenter.Auth;
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

builder.Services.AddSingleton<LocalSecurityTokenValidator>();
builder.Services.AddSingleton<ICacheService, DbCacheSerice>();

builder.Services.AddSingleton<ITransactionalOutboxEventProducer, TransactionalOutboxEventProducer>();
builder.Services.AddSingleton<IOutboxEventPublisher, OutboxEventPublisher>();



builder.Services.AddSingleton<ProfileService>();

builder.Services.AddSingleton<VideoService>();
builder.Services.AddSingleton<VideoUtilities>();

builder.Services.AddSingleton<IMetadataService, MetadataService>();

new AutoControllers().MapControllers(builder.Services);
AutoEventHandlerMapping.MapEventHandlers(builder.Services);

builder.Services.Configure<MetadataOptions>(builder.Configuration.GetSection(MetadataOptions.SectionName));

UserContextSetter setter = new UserContextSetter();
builder.Services.AddSingleton<IUserContextSetter>(setter);
builder.Services.AddSingleton<IUserContextProvider>(setter);

builder.Services.AddSingleton<IUserProfileProvider, UserProfileProvider>();

builder.Services.AddHttpContextAccessor();

// local only, improve later

if(builder.Configuration.GetSection(EventOptions.SectionName)?.GetValue<string>("SqsQueueUrl") != null) {
    builder.Services.Configure<EventOptions>(builder.Configuration.GetSection("Events"));
    builder.Services.AddSingleton<IEventPublisher, SqsEventPublisher>();
    builder.Services.AddSingleton<IEventReceiver, SqsEventReceiver>();
    builder.Services.AddSingleton<AmazonSQSClient>();
} else {
    builder.Services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();
    builder.Services.AddSingleton<IEventReceiver>(serviceProvider => 
            (InMemoryEventPublisher)serviceProvider.GetRequiredService<IEventPublisher>()  );
}

bool inAws = Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME") != null;
builder.Services.AddSingleton<IEventProcessor, EventProcessor>();
if(!inAws) {
    builder.Services.AddHostedService<QueueWatchingService>();
} else {
    builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);
}

var app = builder.Build();
app.UseCors();

if( app.Environment.IsDevelopment() ) {
    app.UseSwagger();
    app.UseSwaggerUI( options => {
        options.EnablePersistAuthorization();
    });
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
