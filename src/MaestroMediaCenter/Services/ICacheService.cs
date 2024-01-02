using Maestro.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maestro.Services;

public record VideoCache(
    Dictionary<string, string> Files,
    Dictionary<string, VideoCache> Folders
);

public interface ICacheService {
    Task<VideoCache> GetCacheAsync(CancellationToken cancellationToken = default);
}

public class DbCacheSerice(IDbContextFactory<MediaDbContext> dbContextFactory) : ICacheService {
    public async Task<VideoCache> GetCacheAsync(CancellationToken cancellationToken = default) {
        // get videos from core table
        using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var videos = await db.Video.ToListAsync(cancellationToken);

        var tvShows = videos.Where(v => v.VideoType == VideoType.TvShow).ToList();

        var moviesMap = videos.Where(v => v.VideoType == VideoType.Movie).ToDictionary(v => v.VideoName, _ => string.Empty);
        var tvShowsMap = new Dictionary<string, VideoCache>();
        foreach(var episode in tvShows) {
            var show = episode.VideoName;
            if(!tvShowsMap.ContainsKey(show)) {
                tvShowsMap.Add(show, new(new Dictionary<string, string>(), new Dictionary<string, VideoCache>()));
            }
            var currentShow = tvShowsMap[show];
        
            var season = "Season " + episode.Season;
            if(season == null) {
                continue;
            }
            if(!currentShow.Folders.ContainsKey(season)) {
                currentShow.Folders.Add(season, new(new Dictionary<string, string>(), new Dictionary<string, VideoCache>()));
            }
            var currentSeason = currentShow.Folders[season];
            var episodeName = episode.Subname;
            if(episodeName == null) {
                continue;
            }
            if(!currentSeason.Files.ContainsKey(episodeName)) {
                currentSeason.Files.Add(episodeName, string.Empty);
            }

        }
        VideoCache root = new(new Dictionary<string, string>(), new Dictionary<string, VideoCache> {
            { "Movies", new(moviesMap, new Dictionary<string, VideoCache>()) },
            { "TV Shows", new(new Dictionary<string, string>(), tvShowsMap) },
            { "Movie Collections", new(new Dictionary<string, string>(), new Dictionary<string, VideoCache>()) }
        });
        return root;
    }
}