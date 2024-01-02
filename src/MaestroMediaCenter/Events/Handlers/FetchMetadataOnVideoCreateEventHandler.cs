using Maestro.Core;
using Maestro.Services;

namespace Maestro.Events;

public class FetchMetadataOnVideoCreateEventHandler(IMetadataService metadataService) : IEventHandler<VideoCreated>
{
    async Task IEventHandler<VideoCreated>.Handle(VideoCreated @event, CancellationToken cancellationToken)
    {
        await metadataService.FetchMetadata(@event.VideoId, cancellationToken);
    }
}