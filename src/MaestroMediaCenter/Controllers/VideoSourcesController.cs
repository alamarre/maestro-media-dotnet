using Maestro.Database;
using Maestro.Models;

namespace Maestro.Controllers;

public class VideoSourcesController : IController {
    private readonly ITable<VideoSources> _videoSources;

    public VideoSourcesController(ITable<VideoSources> videoSources) {
        _videoSources = videoSources;
    }
    public async Task<Videos> AddSource(LocalVideoChange videoChange) {

       return null!;
    }

    void IController.MapRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/videos/sources", AddSource);
    }
}