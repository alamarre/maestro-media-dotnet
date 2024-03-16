using Maestro.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maestro.Controllers;

public class HomepageCollectionsController(IDbContextFactory<MediaDbContext> dbContextFactory) : IController
{
    public async Task<IResult> ListCollections(CancellationToken cancellationToken)
    {
        using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var result = await db.VideoCollection.Select(x => new
        {
            Name = x.VideoCollectionName, StartDate = x.StartDate, EndDate = x.EndDate, Visible = IsVisible(x)
        }).ToListAsync();
        return Results.Ok(result);
    }

    private static bool IsVisible(VideoCollection collection)
    {
        return true;
    }

    public async Task<IResult> GetCollectionItems(string collectionName, CancellationToken cancellationToken)
    {
        using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var videos = await db.VideoCollection
            .Where(vc => vc.VideoCollectionName == collectionName)
            .SelectMany(vc => vc.VideoCollectionItems)
            .Select(vci => vci.Video)
            .Select(v => new { Added = v!.AddedDate, Name = v!.VideoName, Type = GetTypeName(v!.VideoType), })
            .ToListAsync(cancellationToken);

        return Results.Ok();
    }

    private string GetTypeName(VideoType videoType)
    {
        return videoType switch
        {
            VideoType.Movie => "Movie",
            VideoType.TvShow => "TV Show",
            _ => "Unknown"
        };
    }

    public void MapRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/v1.0/homepage_collections", ListCollections);
        routes.MapPost("/api/v1.0/homepage_collections/{collectionName}", GetCollectionItems);
    }
}
