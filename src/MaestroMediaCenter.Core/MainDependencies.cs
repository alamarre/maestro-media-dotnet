using Amazon.SQS;
using Maestro.Auth;
using Maestro.Entities;
using Maestro.Events;
using Maestro.Events.Handlers;
using Maestro.Options;
using Maestro.Services;
using Maestro.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Maestro;

public static class MainDependencies
{
    public static void RegisterMainDependencies(IServiceCollection services, ConfigurationManager configurationManager)
    {
        services.AddSingleton<ICacheService, DbCacheSerice>();
        services.AddSingleton<ITransactionalOutboxEventProducer, TransactionalOutboxEventProducer>();
        services.AddSingleton<IOutboxEventPublisher, OutboxEventPublisher>();
        services.AddSingleton<ProfileService>();
        services.AddSingleton<VideoService>();
        services.AddSingleton<VideoUtilities>();
        services.AddSingleton<IMetadataService, MetadataService>();

        UserContextSetter setter = new UserContextSetter();
        services.AddSingleton<IUserContextSetter>(setter);
        services.AddSingleton<IUserContextProvider>(setter);
        services.AddSingleton<IEventProcessor, EventProcessor>();

        if (configurationManager.GetSection(EventOptions.SectionName)?.GetValue<string>("SqsQueueUrl") != null)
        {
            services.Configure<EventOptions>(configurationManager.GetSection("Events"));
            services.AddSingleton<IEventPublisher, SqsEventPublisher>();
            services.AddSingleton<IEventReceiver, SqsEventReceiver>();
            services.AddSingleton<AmazonSQSClient>();
        }
        else
        {
            services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();
            services.AddSingleton<IEventReceiver>(serviceProvider =>
                (InMemoryEventPublisher)serviceProvider.GetRequiredService<IEventPublisher>());
        }
    }
}
