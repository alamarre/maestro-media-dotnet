using Maestro.Entities;
using Maestro.Events;
using Maestro.Models;
using Maestro.Utilities;
using MaestroMediaCenter.Auth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Maestro.Services;

public record VideoSourcesResponse(List<string> Sources, List<string> Subtitles);

public class VideoService(
    IDbContextFactory<MediaDbContext> dbContextFactory,
    VideoUtilities videoUtilities, 
    ICacheService cacheService,
    ITransactionalOutboxEventProducer eventProducer,
    IUserProfileProvider userProfileProvider) {

    public Task<VideoCache> GetCache() {
        return cacheService.GetCacheAsync();
    }

    public async Task<VideoSourcesResponse?> GetSourcesFromPath(string path) {
        using var db = await dbContextFactory.CreateDbContextAsync();
        var videoInfo = videoUtilities.GetVideoInfo(path);

        if(videoInfo == null) {
            return null;
        }

        if(videoInfo.VideoType == VideoType.Movie) {
            var video = await db.Video.FirstOrDefaultAsync(v => v.VideoName == videoInfo.Name && v.VideoType == videoInfo.VideoType);
            if(video == null) {
                return null;
            }

            var sources = await db.VideoSource.Where(vs => vs.VideoId == video.VideoId).Include(v => v.VideoSourceRoot).ToListAsync();
            var subtitles = sources.Where(s => s.Source.EndsWith(".vtt")).ToList();
            sources = sources.Where(s => !s.Source.EndsWith(".vtt")).ToList();
            var materializedSources = sources.Select(GetUri).ToList();

            return new VideoSourcesResponse(materializedSources, subtitles.Select(GetUri).ToList());
        }

        if(videoInfo.VideoType == VideoType.TvShow && videoInfo is TvShowInfo tvShowInfo) {
            var video = await db.Video.FirstOrDefaultAsync(v => v.VideoName == tvShowInfo.ShowName 
            && v.VideoType == videoInfo.VideoType
            && v.Season == tvShowInfo.Season
            && v.Episode == tvShowInfo.Episode);
            if(video == null) {
                return null;
            }

            var sources = await db.VideoSource.Where(vs => vs.VideoId == video.VideoId).Include(v => v.VideoSourceRoot).ToListAsync();
            var subtitles = sources.Where(s => s.Source.EndsWith(".vtt")).ToList();
            sources = sources.Where(s => !s.Source.EndsWith(".vtt")).ToList();
            var materializedSources = sources.Select(GetUri).ToList();

            return new VideoSourcesResponse(materializedSources, subtitles.Select(GetUri).ToList());
        }
        return null;
    }

    private string GetUri(VideoSource videoSources) {
        if(Uri.TryCreate(new Uri(videoSources.VideoSourceRoot!.VideoSourceRootPath),  videoSources.Source,  out var result)) {
            return result.ToString();
        }

        return "";
    }

    public async Task<VideoSource?> AddSource(LocalVideoChange videoChange, CancellationToken cancellationToken = default) {
        var videoInfo = videoUtilities.GetVideoInfo(videoChange.Path, videoChange.Type, videoChange.RootUrl);
        if(videoInfo == null) {
            return null;
        }
        
        Guid newRootId = Guid.NewGuid();
        using var db = await dbContextFactory.CreateDbContextAsync();
        VideoSource? videoSource = null;
        await db.ExecuteWithRetryAsync(async () => {
            var root = await db.VideoSourceRoot.FirstOrDefaultAsync(r => r.VideoSourceRootPath == videoChange.RootUrl);
            if(root == null) {
                root = new VideoSourceRoot {
                    VideoSourceRootId = newRootId,
                    VideoSourceRootPath = videoChange.RootUrl,
                    VideoSourceLocationType = VideoSourceLocationType.HttpSource
                };
                db.VideoSourceRoot.Add(root);
            }

            var video = await db.Video.FirstOrDefaultAsync(v => v.VideoName == videoInfo.Name && v.VideoType == videoInfo.VideoType);
            if(video == null) {
                video = new Video {
                    VideoId = Guid.NewGuid(),
                    VideoName = videoInfo.Name,
                    VideoType = videoInfo.VideoType
                };

                if(videoInfo.VideoType == VideoType.TvShow && videoInfo is TvShowInfo tvShowInfo) {
                    video.Episode = tvShowInfo.Episode;
                    video.Season = tvShowInfo.Season;
                    video.Subname = tvShowInfo.Subname;
                }
                db.Video.Add(video);

                await eventProducer.Produce(new VideoCreated(video.VideoId), db, cancellationToken);
            }

            videoSource = await db.VideoSource.FirstOrDefaultAsync(vs => 
                vs.VideoSourceRootId == root.VideoSourceRootId 
                && vs.VideoId == video.VideoId 
                && vs.Source == videoChange.Path);
            if(videoSource == null) {
                videoSource = new VideoSource {
                    VideoSourceRootId = root.VideoSourceRootId,
                    VideoId = video.VideoId,
                    Source = videoChange.Path
                };
                db.VideoSource.Add(videoSource);
            }
        });

        return videoSource;
    }

    public async Task<List<ShowProgress>> GetShowProgressesAsync(CancellationToken cancellationToken) {
        using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var result = await db.WatchProgress.Include(w => w.Video).ToListAsync(cancellationToken);

        return result.Select(GetShowProgress).ToList();
    }

    public async Task SaveShowProgressAsync(ShowProgress progress, CancellationToken cancellationToken) {
        using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await db.ExecuteWithRetryAsync(async () => {

            string show = progress.show;
            VideoType videoType = VideoType.TvShow;
            string? season = progress.season;
            string episode = progress.episode;
            if(show.StartsWith("movie_")) {
                show = show.Substring(6);
                videoType = VideoType.Movie;
                episode = show;
            }

            var video = await db.Video.FirstOrDefaultAsync(v => v.VideoName == show && v.VideoType == videoType, cancellationToken);
            if(video == null) {
                return;
            }

            var profile = await userProfileProvider.GetUserProfileAsync(cancellationToken);
            if(profile == null) {
                return;
            }
            var watchProgress = await db.WatchProgress.FirstOrDefaultAsync(w => w.VideoId == video.VideoId && w.ProfileId == profile!.ProfileId, cancellationToken);
            if(watchProgress == null) {
                watchProgress = new WatchProgress {
                    VideoId = video.VideoId,
                    ProfileId = profile!.ProfileId,
                    ProgressInSeconds = progress.progress,
                    Status = progress.status,
                    Expires = new DateTime(progress.expires)
                };
                db.WatchProgress.Add(watchProgress);
            } else {
                watchProgress.ProgressInSeconds = progress.progress;
                watchProgress.Status = progress.status;
                watchProgress.Expires = new DateTime(progress.expires);
            }
        });
    }

    private ShowProgress GetShowProgress(WatchProgress watchProgress) {
        if(watchProgress.Video!.VideoType == VideoType.TvShow) {
            return new ShowProgress(watchProgress.Video.VideoName, watchProgress.Video.Season.ToString(), watchProgress.Video.Episode.ToString()!, watchProgress.Status, watchProgress.ProgressInSeconds, watchProgress.Expires.Ticks);
        }
        string moviePath = $"Movies/{watchProgress.Video.VideoName}";
        return new ShowProgress($"movie_{moviePath}", 
        null, 
        moviePath, 
        watchProgress.Status, 
        watchProgress.ProgressInSeconds, 
        watchProgress.Expires.Ticks);
    }
 
}