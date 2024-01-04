using Maestro.Entities;
using Maestro.Models;
using Maestro.Services;
using Microsoft.EntityFrameworkCore;

namespace Maestro.Controllers;

public class VideosController(VideoService videoService) : IController {

    /// <summary>
    /// This is probably deprecated, but it gets called
    /// </summary>
    /// <returns></returns>
    public IResult Root() {

        const string result = """
[
    {
        "path": "Movies",
        "name": "Movies",
        "index": true
    },
    {
        "path": "TV Shows",
        "name": "TV Shows",
        "type": "TV",
        "index": true
    },
    {
        "name": "Movie Collections",
        "type": "collection",
        "path": ""
    }
]
""";
        return Results.Ok(result);
    }

    public async Task<IResult> GetCache() {
        return Results.Ok(await videoService.GetCache());
    }

    public async Task<IResult> GetShowProgress(CancellationToken cancellationToken) {
        return Results.Ok(await videoService.GetShowProgressesAsync(cancellationToken));
    }

    public async Task<IResult> UpdateShowProgress(ShowProgress showProgress, CancellationToken cancellationToken) {
        await videoService.SaveShowProgressAsync(showProgress, cancellationToken);
        return Results.Ok();
    }

    public async Task<IResult> GetRecentMedia(string mediaType, CancellationToken cancellationToken) {
        return Results.Ok(await videoService.GetCache());
    }

    void IController.MapRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/v1.0/folders/root", Root);
        routes.MapGet("/api/v1.0/cache", GetCache);
        routes.MapGet("/api/v1.0/shows/keep-watching", GetShowProgress);

        routes.MapGet("/api/v1.0/{mediaType}/recent", GetRecentMedia);
        routes.MapPost("/api/v1.0/shows/keep-watching", UpdateShowProgress);
    }
}