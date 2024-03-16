namespace Maestro.Web.Api.Tests;

using System.Net;
using FluentAssertions;
using Maestro.Controllers;
using NUnit.Framework;

public class AuthTests
{
    [Test]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        var result = await AppContext.UnathenticatedClient.PostAsJsonAsync("/login",
            new { Username = "fakeuser", Password = "fakepassword", TenantId = Guid.NewGuid() });

        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Login_WrongTenantId_ReturnsUnauthorized()
    {
        var result = await AppContext.UnathenticatedClient.PostAsJsonAsync("/login",
            new { Username = "fakeadmin", Password = "fakepassword", TenantId = Guid.NewGuid() });

        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Login_Correct_ReturnsValidToken()
    {
        var result = await AppContext.UnathenticatedClient.PostAsJsonAsync("/login",
            new { Username = "fakeadmin", Password = "fakepassword", TenantId = AppContext.ClientTenantId });

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = await result.Content.ReadFromJsonAsync<UserToken>();
        token.Should().NotBeNull();
        token!.Token.Should().NotBeNullOrEmpty();
        token.TenantId.Should().NotBeEmpty();
        token.UserId.Should().NotBeEmpty();
        token.TenantId.Should().Be(AppContext.ClientTenantId);
        token.UserId.Should().Be(AppContext.ClientUserId);
    }
}
