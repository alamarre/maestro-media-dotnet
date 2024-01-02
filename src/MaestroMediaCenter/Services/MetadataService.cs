
using Maestro.Entities;
using Maestro.Options;
using Maestro.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TMDbLib.Client;

namespace Maestro.Services;

public sealed class MetadataService(
    IOptions<MetadataOptions> metadataOptionsProvider, 
    IDbContextFactory<MediaDbContext> dbContextFactory) : IMetadataService
{
    MetadataOptions metadataOptions = metadataOptionsProvider.Value;
    async Task IMetadataService.FetchMetadata(Guid videoId, CancellationToken cancellationToken)
    {
        if(metadataOptions.TmdbKey is null)
        {
            return;
        }
        using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        TMDbClient client = new(metadataOptions.TmdbKey);
        var video = await db.Video.FindAsync(videoId);
        if(video is null)
        {
            return;
        }
        // check if metatadata exists
        var existingMetadata = await db.VideoTmdbMetadata.FirstOrDefaultAsync(x => x.VideoType == video.VideoType && x.VideoName == video.VideoName, cancellationToken);
        if(existingMetadata is not null)
        {
            return;
        }

        var result = VideoUtilities.GetBaseNameAndYear(video.VideoName);
        string name = video.VideoName;
        int year = 0;
        if(result is not null)
        {
            (name, year) = result.Value;
        }

        if(video.VideoType == VideoType.TvShow) {
            var searchResult = await client.SearchTvShowAsync(name, firstAirDateYear: year, cancellationToken: cancellationToken);
            if(searchResult.Results.Count == 0)
            {
                await db.VideoTmdbMetadata.AddAsync( new VideoTmdbMetadata {
                    VideoMetadataId = Guid.NewGuid(),
                    VideoType = VideoType.TvShow,
                    VideoName = video.VideoName,
                    TmdbId = -1
                }, cancellationToken);
                return;
            }

            // TODO: filter by name and year match
            var firstResult = searchResult.Results[0];
            await db.VideoTmdbMetadata.AddAsync( new VideoTmdbMetadata {
                VideoMetadataId = Guid.NewGuid(),
                VideoType = VideoType.TvShow,
                VideoName = video.VideoName,
                TmdbId = firstResult.Id
            }, cancellationToken);
        } else if(video.VideoType == VideoType.Movie) {
            var searchResult = await client.SearchMovieAsync(name, year, cancellationToken: cancellationToken);
            if(searchResult.Results.Count == 0)
            {
                await db.VideoTmdbMetadata.AddAsync( new VideoTmdbMetadata {
                    VideoMetadataId = Guid.NewGuid(),
                    VideoType = VideoType.Movie,
                    VideoName = video.VideoName,
                    TmdbId = -1
                }, cancellationToken);
                return;
            }

            // TODO: filter by name and year match
            var firstResult = searchResult.Results[0];
            await db.VideoTmdbMetadata.AddAsync( new VideoTmdbMetadata {
                VideoMetadataId = Guid.NewGuid(),
                VideoType = VideoType.Movie,
                VideoName = video.VideoName,
                TmdbId = firstResult.Id
            }, cancellationToken);
        }
    }
}
