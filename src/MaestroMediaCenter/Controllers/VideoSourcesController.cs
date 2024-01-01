using Maestro.Database;
using Maestro.Models;
using Maestro.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Maestro.Controllers;

public class VideoSourcesController(VideoService videoService) : IController {

    public async Task<IResult> AddSource(LocalVideoChange videoChange) {
        VideoSources? response = await videoService.AddSource(videoChange);

        if(response == null) {
            return Results.Problem();
        }
        return Results.Ok(response);
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
        routes.MapPost("/api/videos/sources", AddSource);
        routes.MapGet("/api/v1.0/videos/sources", GetSources);
    }
}