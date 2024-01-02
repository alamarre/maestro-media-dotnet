using Maestro.Entities;
using NUnit.Framework;
using FluentAssertions;
using Maestro.Services;
using Maestro.Models;

namespace Maestro.Web.Api.Tests;

public class VideoSourcesTests {

    const string rootUrl = "http://localhost:5000";
    [Test]
    public async Task AddSource_NewSource_ReturnsVideo() {
        
        string randomFile = Guid.NewGuid().ToString();
        string path = $"TV Shows/Show/Season 1/{randomFile}.mp4";
        var result = await AppContext.Client.PostAsJsonAsync("/api/videos/sources", new LocalVideoChange(
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

     [Test]
    public async Task AddSource_SameSourceTwice_ReturnsOriginalVideo() {
        string randomFile = Guid.NewGuid().ToString();
        string path = $"TV Shows/Show/Season 1/{randomFile}.mp4";
        var result = await AppContext.Client.PostAsJsonAsync("/api/videos/sources", new LocalVideoChange(
            "tv",
            rootUrl,
            path
        ));
        result.EnsureSuccessStatusCode();
        var responseBody = await result.Content.ReadFromJsonAsync<VideoSource>();
        responseBody.Should().NotBeNull();
        responseBody!.VideoId.Should().NotBeEmpty();
        responseBody.Source.Should().NotBeNullOrEmpty();

        var result2 = await AppContext.Client.PostAsJsonAsync("/api/videos/sources", new LocalVideoChange(
            "tv",
            rootUrl,
            path
        ));

        var secondResponseBody = await result2.Content.ReadFromJsonAsync<VideoSource>();
        secondResponseBody.Should().NotBeNull();
        secondResponseBody.Should().BeEquivalentTo(responseBody);
    }

    [Test]
    public async Task AddTvSource_GetVideo_VideoHasCorrectInfo() {
        string randomFile = "S02E02 - "+Guid.NewGuid().ToString();
        var showName = Guid.NewGuid().ToString();
        var season = "Season 2";
        string path = $"TV Shows/{showName}/{season}/{randomFile}.mp4";
        var result = await AppContext.Client.PostAsJsonAsync("/api/videos/sources", new LocalVideoChange(
            "tv",
            rootUrl,
            path
        ));
        result.EnsureSuccessStatusCode();
        var responseBody = await result.Content.ReadFromJsonAsync<VideoSource>();
        responseBody.Should().NotBeNull();
        responseBody!.VideoId.Should().NotBeEmpty();
        responseBody.Source.Should().NotBeNullOrEmpty();

        var cache = await AppContext.Client.GetFromJsonAsync<VideoCache>("/api/v1.0/cache");
        cache.Should().NotBeNull();
        cache!.Folders.Should().ContainKey("TV Shows");
        var tvShows = cache.Folders["TV Shows"];
        tvShows.Folders.Should().ContainKey(showName);
        var show = tvShows.Folders[showName];
        show.Folders.Should().ContainKey(season);
        var seasonFolder = show.Folders[season];
        seasonFolder.Files.Should().ContainKey(randomFile);


    }

     [Test]
    public async Task AddMovieSource_GetVideo_VideoHasCorrectInfo() {
        string randomFile = Guid.NewGuid().ToString();

        string path = $"Movies/SomeDirectory/{randomFile}.mp4";
        var result = await AppContext.Client.PostAsJsonAsync("/api/videos/sources", new LocalVideoChange(
            "movie",
            rootUrl,
            path
        ));
        result.EnsureSuccessStatusCode();
        var responseBody = await result.Content.ReadFromJsonAsync<VideoSource>();
        responseBody.Should().NotBeNull();
        responseBody!.VideoId.Should().NotBeEmpty();
        responseBody.Source.Should().NotBeNullOrEmpty();

        var cache = await AppContext.Client.GetFromJsonAsync<VideoCache>("/api/v1.0/cache");
        cache.Should().NotBeNull();
        cache!.Folders.Should().ContainKey("Movies");
        var movies = cache.Folders["Movies"];
        movies.Files.Should().ContainKey(randomFile);

        // extension and subdirectory should be stripped
        var internalPath = $"Movies/{randomFile}";
        var sources = await AppContext.Client.GetFromJsonAsync<VideoSourcesResponse>($"/api/v1.0/videos/sources?path={internalPath}");

        sources.Should().NotBeNull();

        sources!.Sources.Should().Contain($"{rootUrl}/{path}");
    }

     [Test]
    public async Task AddMovieMultipleSources_GetVideo_VideoHasCorrectInfo() {
        string randomFile = Guid.NewGuid().ToString();

        var extensions = new string[] { ".mp4", ".mkv", ".vtt"  };
        foreach(var extension in extensions) {
            string path = $"Movies/SomeDirectory/{randomFile}{extension}";
            var result = await AppContext.Client.PostAsJsonAsync("/api/videos/sources", new LocalVideoChange(
                "movie",
                rootUrl,
                path
            ));
            result.EnsureSuccessStatusCode();
            var responseBody = await result.Content.ReadFromJsonAsync<VideoSource>();
            responseBody.Should().NotBeNull();
            responseBody!.VideoId.Should().NotBeEmpty();
            responseBody.Source.Should().NotBeNullOrEmpty();
        }

        var cache = await AppContext.Client.GetFromJsonAsync<VideoCache>("/api/v1.0/cache");
        cache.Should().NotBeNull();
        cache!.Folders.Should().ContainKey("Movies");
        var movies = cache.Folders["Movies"];
        movies.Files.Should().ContainKey(randomFile);

        // extension and subdirectory should be stripped
        var internalPath = $"Movies/{randomFile}";
        var sources = await AppContext.Client.GetFromJsonAsync<VideoSourcesResponse>($"/api/v1.0/videos/sources?path={internalPath}");

        sources.Should().NotBeNull();

        sources!.Sources.Should().Contain($"{rootUrl}/Movies/SomeDirectory/{randomFile}{extensions[0]}");
        sources!.Sources.Should().Contain($"{rootUrl}/Movies/SomeDirectory/{randomFile}{extensions[1]}");
        sources!.Subtitles.Should().Contain($"{rootUrl}/Movies/SomeDirectory/{randomFile}{extensions[2]}");
    }
}