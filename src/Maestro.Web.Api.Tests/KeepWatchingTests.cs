using Maestro.Entities;
using NUnit.Framework;
using FluentAssertions;
using Maestro.Models;

namespace Maestro.Web.Api.Tests;

public class KeepWatchingTests
{
    const string profileName = "keepwatching";
    const string rootUrl = "http://localhost:5000";

    [OneTimeSetUp]
    public async Task Setup()
    {
        var result = await AppContext.Client.PostAsJsonAsync("/api/v1.0/profiles", new UserProfile(
            profileName,
            false
        ));
        result.EnsureSuccessStatusCode();
    }

    [Test]
    public async Task AddSource_NewSource_ReturnsVideo()
    {
        string randomFile = Guid.NewGuid().ToString();
        string path = $"TV Shows/Show/Season 1/{randomFile}.mp4";
        var result = await AppContext.Client.PostAsJsonAsync("/api/v1.0/videos/source", new LocalVideoChange(
            "tv",
            rootUrl,
            path
        ));
        result.EnsureSuccessStatusCode();
        var responseBody = await result.Content.ReadFromJsonAsync<VideoSource>();
        responseBody.Should().NotBeNull();
        responseBody!.VideoId.Should().NotBeEmpty();
        responseBody.Source.Should().NotBeNullOrEmpty();
    }
}
