using Microsoft.Extensions.DependencyInjection;

namespace Maestro.Events;

public static partial class AutoEventHandlerMapping
{
    public static void MapEventHandlers(IServiceCollection services)
    {
        RegisterEventHandlers(services);
    }

    static partial void RegisterEventHandlers(IServiceCollection services);
}
