namespace Maestro.Controllers;

public static class SampleController
{
    public static string Root()
    {
        return "Hello World!";
    }

    public static void MapRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapGet("/", Root);
        routes.MapGet("/root", Root);
    }
}
