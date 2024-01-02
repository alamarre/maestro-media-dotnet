using Maestro.Entities;
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

    public async Task<IResult> GetShowProgress() {
        return Results.Ok(await videoService.GetCache());
    }

    void IController.MapRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/v1.0/folders/root", Root);
        routes.MapGet("/api/v1.0/cache", GetCache);
        routes.MapGet("/api/v1.0/shows/keep-watching", GetShowProgress);
    }
}