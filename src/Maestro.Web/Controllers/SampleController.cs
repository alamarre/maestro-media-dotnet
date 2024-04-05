namespace Maestro.Controllers;

public class SampleController : IController
{
    private string Root()
    {
        return "Hello World!";
    }

    public void MapRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/", Root);
        routes.MapGet("/root", Root);
    }
}
