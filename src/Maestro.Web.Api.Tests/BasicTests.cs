using System.Net;
using FluentAssertions;
using NUnit.Framework;

namespace Maestro.Web.Api.Tests;

public class BasicTests
{
    [Test]
    public async Task Ping_Call_ReturnsPong()
    {
        var result = await AppContext.Client.GetAsync("/ping");
        result.EnsureSuccessStatusCode();
        var responseBody = await result.Content.ReadAsStringAsync();
        responseBody.Should().Be("pong");
    }

    [Test]
    public async Task Pong_FakeUrl_Returns404()
    {
        var result = await AppContext.Client.GetAsync("/pong");
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
