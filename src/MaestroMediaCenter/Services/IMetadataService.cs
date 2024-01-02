namespace Maestro.Services;

public interface IMetadataService {
    Task FetchMetadata(Guid VideoId, CancellationToken cancellationToken = default);
}