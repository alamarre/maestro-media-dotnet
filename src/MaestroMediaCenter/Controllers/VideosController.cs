using Maestro.Database;
using Maestro.Models;
using Microsoft.EntityFrameworkCore;

namespace Maestro.Controllers;

public class VideosController(IDbContextFactory<MediaDbContext> dbContextFactory) : IController {

    public async Task<Videos> Root() {
        using var db = await dbContextFactory.CreateDbContextAsync();
        var result = await db.Videos.FirstOrDefaultAsync(x => x.VideoId == Guid.Empty);

        return result!;
    }

    void IController.MapRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/videos", Root);
    }
}