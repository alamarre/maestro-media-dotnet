using Maestro.Models;
using Maestro.Utilities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Maestro.Services;

public class VideoService(IDbContextFactory<MediaDbContext> dbContextFactory, VideoUtilities videoUtilities) {


    public async Task<VideoSources?> AddSource(LocalVideoChange videoChange) {
        var videoInfo = videoUtilities.GetVideoInfo(videoChange.Path, videoChange.Type, videoChange.RootUrl);
        if(videoInfo == null) {
            return null;
        }
        
        Guid newRootId = Guid.NewGuid();
        using var db = await dbContextFactory.CreateDbContextAsync();
        VideoSources? videoSource = null;
        await db.ExecuteWithRetryAsync(async () => {
            var root = await db.VideoSourceRoots.FirstOrDefaultAsync(r => r.VideoSourceRootPath == videoChange.RootUrl);
            if(root == null) {
                root = new VideoSourceRoots {
                    VideoSourceRootId = newRootId,
                    VideoSourceRootPath = videoChange.RootUrl,
                    VideoSourceLocationType = VideoSourceLocationType.HttpSource
                };
                db.VideoSourceRoots.Add(root);
            }

            var video = await db.Videos.FirstOrDefaultAsync(v => v.VideoName == videoInfo.Name && v.VideoType == videoInfo.VideoType);
            if(video == null) {
                video = new Videos {
                    VideoId = Guid.NewGuid(),
                    VideoName = videoInfo.Name,
                    VideoType = videoInfo.VideoType
                };
                db.Videos.Add(video);
            }

            videoSource = await db.VideoSources.FirstOrDefaultAsync(vs => 
                vs.VideoSourceRootId == root.VideoSourceRootId 
                && vs.VideoId == video.VideoId 
                && vs.Source == videoChange.Path);
            if(videoSource == null) {
                videoSource = new VideoSources {
                    VideoSourceRootId = root.VideoSourceRootId,
                    VideoId = video.VideoId,
                    Source = videoChange.Path
                };
                db.VideoSources.Add(videoSource);
            }
        });

        return videoSource;
    }
    public async Task<VideoSources?> AddSourceOld(LocalVideoChange videoChange) {
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
        var videoInfo = videoUtilities.GetVideoInfo(videoChange.Path, videoChange.Type, videoChange.RootUrl);
        if(videoInfo == null) {
            return null;
        }
        string videoName = videoInfo.Name;
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

        return videoSource;
    }
 
}