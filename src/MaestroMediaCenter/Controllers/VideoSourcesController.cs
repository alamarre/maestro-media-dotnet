using Maestro.Database;
using Maestro.Models;

namespace Maestro.Controllers;

public class VideoSourcesController : IController {
    private readonly ITable<VideoSources> _videoSources;
    private readonly ITable<VideoSourceRoots> _videoSourceRoots;

    public VideoSourcesController(ITable<VideoSources> videoSources, ITable<VideoSourceRoots> videoSourceRoots) {
        _videoSources = videoSources;
        _videoSourceRoots = videoSourceRoots;
    }

    public async Task<Videos> AddSource(LocalVideoChange videoChange) {
        
        var root = await _videoSourceRoots.GetOrCreateAsync(
            videoSourceRoot => videoSourceRoot.VideoSourceRootPath == videoChange.RootUrl,
            (root) => root with  {
                VideoSourceRootPath = videoChange.RootUrl,
                VideoSourceRootType = VideoSourceRootType.HttpSource
            }
        );

        /*await _videoSources.GetOrCreateAsync(
            videoSource => videoSource.VideoSourceRootId == root.VideoSourceRootId && videoSource.VideoSourcePath == videoChange.VideoPath,
            (videoSource) => videoSource with {
                VideoSourceRootId = root.VideoSourceRootId
            }
        );*/
        
        return null!;
    }

    void IController.MapRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/videos/sources", AddSource);
    }
}