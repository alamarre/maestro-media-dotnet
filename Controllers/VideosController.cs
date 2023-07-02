using Maestro.Database;
using Maestro.Models;

namespace Maestro.Controllers;

public class VideosController : IController {
    private readonly ITable<Videos> _videos;

    public VideosController(ITable<Videos> videos) {
        _videos = videos;
    }
    public async Task<Videos> Root() {

        var result =await _videos.UpsertAsync(
            video => video.Name == "Test Video 2", 
            video => {
                var x = video with { 
                    VideoType = VideoType.TVShows,
                    Name = "Test Video 2" 
                };
                var y = x == video;
                return x;
            }
        );

        return result;
    }

    void IController.MapRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/videos", Root);
    }
}