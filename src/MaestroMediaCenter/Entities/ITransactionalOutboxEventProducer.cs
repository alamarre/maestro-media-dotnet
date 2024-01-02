using Microsoft.EntityFrameworkCore;

namespace Maestro.Entities;

public interface ITransactionalOutboxEventProducer {
    Task Produce<T>(T @event, MediaDbContext dbContext, CancellationToken cancellationToken = default) where T : notnull;
}