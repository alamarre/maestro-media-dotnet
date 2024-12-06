using Maestro.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Maestro.Controllers;

public record AlternateMediaServer(string Hostname, int Port, string OriginalUrl);

public record AlternateMediaServerLegacy(string Ip, int Port);

public class ServersController(IDbContextFactory<MediaDbContext> dbContextFactory) : IController
{
    public async Task<IResult> ListServers(CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var result = await db.VideoServerAlternate.Select(a => new AlternateMediaServerLegacy(a.Hostname, a.Port))
            .ToListAsync();
        return Results.Ok(result);
    }

    public async Task<IResult> CreateServer(AlternateMediaServer server, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await db.ExecuteWithRetryAsync(async () =>
        {
            var root = await db.VideoSourceRoot.FirstOrDefaultAsync(v => v.VideoSourceRootPath == server.OriginalUrl);
            if (root == null)
            {
                root = new VideoSourceRoot
                {
                    VideoSourceRootId = Guid.NewGuid(),
                    VideoSourceRootPath = server.OriginalUrl,
                    VideoSourceLocationType = VideoSourceLocationType.HttpSource
                };
                await db.VideoSourceRoot.AddAsync(root, cancellationToken);
            }

            var existing =
                await db.VideoServerAlternate.FirstOrDefaultAsync(a =>
                    a.Hostname == server.Hostname && a.Port == server.Port);
            if (existing != null)
            {
                return;
            }

            var alternate = new VideoServerAlternate
            {
                VideoServerAlternateId = Guid.NewGuid(),
                VideoSourceRootId = root.VideoSourceRootId,
                Hostname = server.Hostname,
                Port = server.Port
            };

            db.VideoServerAlternate.Add(alternate);
            await db.SaveChangesAsync(cancellationToken);
        });
        return Results.Ok();
    }

    public void MapRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/v1.0/servers", ListServers);
        routes.MapPost("/api/v1.0/servers", CreateServer);
    }
}
