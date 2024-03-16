using Microsoft.AspNetCore.Authorization;

namespace Maestro.Controllers;

public class PingController : IController
{
    [AllowAnonymous]
    public string Ping()
    {
        return "pong";
    }

    public void MapRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/ping", Ping);
    }
}
