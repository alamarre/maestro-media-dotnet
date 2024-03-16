using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Maestro.Auth;
using Maestro.Controllers;
using Maestro.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Database.Hosting;
using TestContainers.Container.Database.PostgreSql;

namespace Maestro.Web.Api.Tests;

[SetUpFixture]
public class AppContext
{
    private static PostgreSqlContainer? Container { get; set; }

    private static WebApplicationFactory<Program>? App { get; set; }

    private static HttpClient? _adminClient;
    private static HttpClient? _client;

    public static HttpClient UnathenticatedClient => App!.CreateClient();

    public static HttpClient AdminClient => _adminClient!;
    public static HttpClient Client => _client!;

    public static Guid ClientTenantId { get; private set; }

    public static Guid ClientUserId { get; private set; }

    public static string? AdminToken { get; private set; }

    public static string? LocalAdminToken { get; }


    [OneTimeSetUp]
    public async Task InitializeAsync()
    {
        var container = new ContainerBuilder<PostgreSqlContainer>()
            .ConfigureDatabaseConfiguration("postgres", "postgres", "postgres")
            .Build();

        await container.StartAsync();

        Environment.SetEnvironmentVariable("CONNECTION_STRING", container.GetConnectionString());
        const string secretKey = "TestOnlySecretKey";

        Environment.SetEnvironmentVariable("JWT_SECRET", secretKey);
        App = new WebApplicationFactory<Program>();

        _adminClient = App.CreateClient();
        _adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AdminToken);

        var scope = App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
        db.Database.EnsureCreated();

        var tokenizer = App.Services.GetRequiredService<LocalSecurityTokenValidator>();
        AdminToken = tokenizer.CreateToken(
            Guid.NewGuid(),
            additionalClaims: new Dictionary<string, string> { { "gadm", "true" } },
            secretKey: secretKey);
        Container = container;

        string fakeUsername = "fakeadmin";
        string fakePassword = "fakepassword";
        var request = new HttpRequestMessage(HttpMethod.Post, "/tenant/localhost")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    email = "fake@example.com", password = fakePassword, username = fakeUsername
                }), Encoding.UTF8, "application/json")
        };

        var result = await _adminClient.SendAsync(request);
        result.EnsureSuccessStatusCode();

        var tenantDomain = await db.TenantDomain.SingleAsync(x => x.DomainName == "localhost");
        var userContext = new UserContext(
            tenantDomain.TenantId,
            null,
            false,
            false,
            false
        );

        var client = App.CreateClient();
        var loginRequest = new HttpRequestMessage(HttpMethod.Post, "/login")
        {
            Content = new StringContent(JsonSerializer.Serialize(new Credentials
            (
                fakeUsername,
                fakePassword,
                tenantDomain.TenantId
            )), Encoding.UTF8, "application/json")
        };

        result = await client.SendAsync(loginRequest);
        result.EnsureSuccessStatusCode();
        var token = await result.Content.ReadFromJsonAsync<UserToken>();
        ClientTenantId = tenantDomain.TenantId;
        ClientUserId = token!.UserId;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.Token);
        _client = client;
    }

    [OneTimeTearDown]
    public async Task DisposeAsync()
    {
        await Container!.StopAsync();
        App?.Dispose();
        _adminClient?.Dispose();
        _client?.Dispose();
    }
}
