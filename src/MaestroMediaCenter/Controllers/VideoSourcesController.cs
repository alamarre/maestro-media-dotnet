using Maestro.Database;
using Maestro.Models;
using Microsoft.EntityFrameworkCore;

namespace Maestro.Controllers;

public class VideoSourcesController(IDbContextFactory<MediaDbContext> dbContextFactory) : IController {

    public async Task<Videos> AddSource(LocalVideoChange videoChange) {
        
        Guid newRootId = Guid.NewGuid();
        using var db = await dbContextFactory.CreateDbContextAsync();
        var root = await db.VideoSourceRoots.GetOrCreateAsync<VideoSourceRoots>(
            db,
            videoSourceRoot => videoSourceRoot.VideoSourceRootPath == videoChange.RootUrl,
            (VideoSourceRoots root) => root with  {
                VideoSourceRootId = newRootId,
                VideoSourceRootPath = videoChange.RootUrl,
                VideoSourceLocationType = VideoSourceLocationType.HttpSource
            }
        );

        if(newRootId == root.VideoSourceRootId) {
            // new video source root created
        }
        string videoName = "extract this";
        var videoType = VideoType.Movie;
        Guid newVideoId = Guid.NewGuid();

        var video = await db.Videos.GetOrCreateAsync<Videos>(
            db,
            video => video.VideoName == videoName && video.VideoType == videoType,
            (Videos video) => video with {
                VideoId = newVideoId,
                VideoType = videoType
            }
        );

        if(video.VideoId == newVideoId) {
            // new video created
        }

        var videoSource = await db.VideoSources.GetOrCreateAsync<VideoSources>(
            db,
            videoSource => videoSource.VideoSourceRootId == root.VideoSourceRootId && videoSource.VideoId == video.VideoId && videoSource.Source == videoChange.Path,
            (videoSource) => videoSource with {
                VideoSourceRootId = root.VideoSourceRootId,
                VideoId = video.VideoId,
                Source = videoChange.Path
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