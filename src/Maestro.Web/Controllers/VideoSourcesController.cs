using Maestro.Entities;
using Maestro.Models;
using Maestro.Services;
using Microsoft.AspNetCore.Mvc;

namespace Maestro.Controllers;

public class VideoSourcesController(VideoService videoService) : IController {

    public async Task<IResult> AddSource([FromBody] LocalVideoChange videoChange) {
        VideoSource? response = await videoService.AddSource(videoChange);

        if(response == null) {
            return Results.Problem();
        }
        return Results.Ok(response);
    }

    public async Task<IResult> DeleteSource([FromBody] LocalVideoChange videoChange, CancellationToken cancellationToken) {
        await videoService.DeleteSource(videoChange);

        return Results.Ok();
    }

    public async Task<IResult> GetSources([FromQuery] string path) {
        var sources = await videoService.GetSourcesFromPath(path);
        if(sources == null) {
            return Results.NotFound();
        }
        return Results.Ok(sources);
    }

    void IController.MapRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/v1.0/videos/source", AddSource);
        routes.MapDelete("/api/v1.0/videos/source", DeleteSource);
        routes.MapGet("/api/v1.0/folders/sources", GetSources);
    }
}