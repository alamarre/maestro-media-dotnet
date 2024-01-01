using Maestro.Auth;
using Maestro.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Maestro.Controllers;

public class PlaylistsController(IUserContextProvider userContextProvider) : IController {
    public IResult GetPlaylists(CancellationToken cancellationToken) {
        var context = userContextProvider.GetUserContext();
        if(context == null) {
            return Results.NotFound();
        }
        var result = new { AccountId = context.TenantId };
        return Results.Ok(result);
    }

    public void MapRoutes(IEndpointRouteBuilder routes) {
        routes.MapGet("/api/v1.0/playlists", GetPlaylists);
    }
}
