namespace Maestro.Controllers;

public partial class AutoControllers
{
    public void MapControllers(IServiceCollection services)
    {
        RegisterControllers(services);
    }

    partial void RegisterControllers(IServiceCollection services);
}
