namespace Maestro.Web.Api.Tests;

using System.Net;
using FluentAssertions;
using Maestro.Controllers;

[Collection("Database collection")]
public class AuthTests(AppFixture app)
{
    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        var result = await app.UnathenticatedClient.PostAsJsonAsync("/login", new {
            Username = "fakeuser",
            Password = "fakepassword",
            TenantId = Guid.NewGuid()
        });

        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WrongTenantId_ReturnsUnauthorized()
    {
        var result = await app.UnathenticatedClient.PostAsJsonAsync("/login", new {
            Username = "fakeadmin",
            Password = "fakepassword",
            TenantId = Guid.NewGuid()
        });

        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_Correct_ReturnsValidToken()
    {
        var result = await app.UnathenticatedClient.PostAsJsonAsync("/login", new {
            Username = "fakeadmin",
            Password = "fakepassword",
            TenantId = app.ClientTenantId
        });

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = await result.Content.ReadFromJsonAsync<UserToken>();
        token.Should().NotBeNull();
        token!.Token.Should().NotBeNullOrEmpty();
        token.TenantId.Should().NotBeEmpty();
        token.UserId.Should().NotBeEmpty();
        token.TenantId.Should().Be(app.ClientTenantId);
        token.UserId.Should().Be(app.ClientUserId);
    }
}