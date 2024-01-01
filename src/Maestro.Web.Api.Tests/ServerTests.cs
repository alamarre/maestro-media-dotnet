using Maestro.Models;
using NUnit.Framework;
using FluentAssertions;
using Maestro.Controllers;

namespace Maestro.Web.Api.Tests;

public class ServerTests
{
    [Test]
    public async Task CreateServer_NewServer_ReturnsServer() {
        var server = new AlternateMediaServer(
            Hostname: "127.0.0.1",
            Port: 5000,
            OriginalUrl: "http://localhost:5000"
        );
        var result = await AppContext.Client.PostAsJsonAsync("/api/v1.0/servers", server);
        result.EnsureSuccessStatusCode();
        var responseBody = await AppContext.Client.GetFromJsonAsync<List<AlternateMediaServerLegacy>>("/api/v1.0/servers");
        responseBody.Should().NotBeNull();
        responseBody!.First().Ip.Should().Be(server.Hostname);
        responseBody!.First().Port.Should().Be(server.Port);
    }

    [Test]
    public async Task CreateServer_SameServerTwice_ReturnsOriginalServer() {
         var server = new AlternateMediaServer(
            Hostname: "127.0.0.1",
            Port: 5000,
            OriginalUrl: "http://localhost:5000"
        );
        var result = await AppContext.Client.PostAsJsonAsync("/api/v1.0/servers", server);
        result.EnsureSuccessStatusCode();
        var responseBody = await AppContext.Client.GetFromJsonAsync<List<AlternateMediaServerLegacy>>("/api/v1.0/servers");
        responseBody.Should().NotBeNull();
        responseBody!.First().Ip.Should().Be(server.Hostname);
        responseBody!.First().Port.Should().Be(server.Port);

        var result2 = await AppContext.Client.PostAsJsonAsync("/api/v1.0/servers", server);
        result2.EnsureSuccessStatusCode();

        var secondResponseBody = await AppContext.Client.GetFromJsonAsync<List<AlternateMediaServerLegacy>>("/api/v1.0/servers");
        secondResponseBody.Should().NotBeNull();
        secondResponseBody.Should().BeEquivalentTo(responseBody);
    }
}
