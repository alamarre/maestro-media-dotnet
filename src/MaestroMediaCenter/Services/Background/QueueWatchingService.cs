using System.Reflection;
using System.Text.Json;
using Maestro.Events;
using Maestro.Events.Handlers;

namespace Maestro.Services.Background;

public sealed class QueueWatchingService( 
    ILogger<QueueWatchingService> logger,
    IEventReceiver eventReceiver,
    IEventProcessor eventProcessor
) : BackgroundService {
   
    MethodInfo? method = typeof(IEventProcessor).GetMethod(nameof(IEventProcessor.ProcessEvent), 
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        if(method == null) {
            throw new InvalidOperationException("Method not found");
        }

        await Task.Yield();
        
        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            try {
                var receivedEvent = await eventReceiver.Receive(stoppingToken);
                if (receivedEvent is null) {
                    continue;
                }
                
                logger.LogInformation("Received event: {EventType}", receivedEvent.EventTypeId);
                var eventType = AutoEventMapping.GetEventType(receivedEvent.EventTypeId);
                if(eventType == null) {
                    logger.LogError("Event type not found: {EventTypeId}", receivedEvent.EventTypeId);
                    continue;
                }
                var genericMethod = method!.MakeGenericMethod(eventType);
                if(genericMethod == null) {
                    logger.LogError("Generic method not found: {EventTypeId}", receivedEvent.EventTypeId);
                    continue;
                }

                var result = genericMethod.Invoke(eventProcessor, new object[] { receivedEvent.EventData, stoppingToken }) as Task;
                
                if(result == null) {
                    logger.LogError("Failed to get task from runner {eventTypeId}", receivedEvent.EventTypeId);
                    continue;
                }
                await result;  

                await eventReceiver.DeleteEvent(receivedEvent, stoppingToken);
            } catch(Exception ex) {
                logger.LogError(ex, "Failed to process event");
            }      
        }
    }
}