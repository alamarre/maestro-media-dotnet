using System.Reflection;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SQSEvents;
using Maestro;
using Maestro.Entities;
using Maestro.Events;
using Maestro.Events.Handlers;
using Maestro.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Configure Host Builder and Services
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddDbContextFactory<MediaDbContext>();
AutoEventHandlerMapping.MapEventHandlers(builder.Services);
MainDependencies.RegisterMainDependencies(builder.Services, builder.Configuration);
var app = builder.Build();

await app.StartAsync();

var processor = app.Services.GetRequiredService<IEventProcessor>();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

MethodInfo? method = typeof(IEventProcessor).GetMethod(nameof(IEventProcessor.ProcessEvent), 
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

// Start the application
var lambdaBuilder = LambdaBootstrapBuilder.Create(async (SQSEvent @event, ILambdaContext context) => {
    foreach(var message in @event.Records) {
        var body = message.Body;
        var eventMessage = JsonSerializer.Deserialize<EventMessage>(body);
        if(eventMessage == null) {
            throw new NotSupportedException("Could not deserialize event message");
        }
        var receivedEvent = AutoEventMapping.ConvertToReceivedMessage(eventMessage) ;
        if(receivedEvent == null) {
            throw new NotSupportedException("Could not deserialize event message");
        }

        var eventType = AutoEventMapping.GetEventType(receivedEvent.EventTypeId);
        if(eventType == null) {
            logger.LogError("Event type not found: {EventTypeId}", receivedEvent.EventTypeId);
            throw new NotSupportedException("Could not deserialize event message");
        }
        var genericMethod = method!.MakeGenericMethod(eventType);
        if(genericMethod == null) {
            logger.LogError("Generic method not found: {EventTypeId}", receivedEvent.EventTypeId);
            throw new NotSupportedException("Could not deserialize event message");
        }

        CancellationToken cancellationToken = default;
        var result = genericMethod.Invoke(processor, new object[] { receivedEvent.EventData, cancellationToken }) as Task;
        
        if(result == null) {
            logger.LogError("Failed to get task from runner {eventTypeId}", receivedEvent.EventTypeId);
            throw new NotSupportedException("Could not deserialize event message");
        }
        await result;
    }

}, new DefaultLambdaJsonSerializer());

await lambdaBuilder.Build().RunAsync();
