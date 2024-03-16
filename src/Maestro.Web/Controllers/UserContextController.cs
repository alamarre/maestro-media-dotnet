using Maestro.Auth;
using Maestro.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Maestro.Controllers;

public class UserContextController(IUserContextProvider userContextProvider) : IController
{
    public IResult GetAccount(CancellationToken cancellationToken)
    {
        var context = userContextProvider.GetUserContext();
        if (context == null)
        {
            return Results.NotFound();
        }

        var result = new { AccountId = context.TenantId };
        return Results.Ok(result);
    }

    public void MapRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/v1.0/account", GetAccount);
    }
}
