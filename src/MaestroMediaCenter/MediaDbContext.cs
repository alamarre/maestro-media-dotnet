using Maestro.Auth;
using Maestro.Events;
using Microsoft.EntityFrameworkCore;

namespace Maestro.Entities;

public partial class MediaDbContext(
    DbContextOptions<MediaDbContext> options,
    IUserContextProvider userContextProvider,
    IOutboxEventPublisher eventPublisher) : DbContext(options)
{
    // this is scoped so it should have the tenant set
    public Guid? TenantId = userContextProvider.GetUserContext()?.TenantId;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
        optionsBuilder.UseNpgsql(connectionString!);
        optionsBuilder.EnableSensitiveDataLogging();
        //optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //modelBuilder.Entity<AccountLogins>().HasQueryFilter(e => TenantId != null && TenantId == e.TenantId );
        MapQueryFilters(modelBuilder);
    }

    partial void MapQueryFilters(ModelBuilder modelBuilder);

    private void AddQueryFilter<T>(ModelBuilder modelBuilder) where T : TenantTable
    {
        modelBuilder.Entity<T>().HasQueryFilter(e =>
            TenantId != null
            && e.TenantId == TenantId
            && !e.SoftDeleted
        );
    }


    public override int SaveChanges()
    {
        SetTenantIdForAddedEntities();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTenantIdForAddedEntities();

        List<OutboxEvent>? outboxEvents = null;
        if (this.Database.CurrentTransaction != null)
        {
            outboxEvents = ChangeTracker
                .Entries<OutboxEvent>()
                .Where(e => e.State == EntityState.Added)
                .Select(e => e.Entity)
                .ToList();
        }

        int result = await base.SaveChangesAsync(cancellationToken);

        if (outboxEvents != null && this.Database.CurrentTransaction != null)
        {
            await this.Database.CurrentTransaction.CommitAsync(cancellationToken);

            await eventPublisher.Publish(outboxEvents, cancellationToken);
        }

        return result;
    }

    private void SetTenantIdForAddedEntities()
    {
        var addedEntities = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added && e.Entity is TenantTable);

        if (TenantId == null)
        {
            return;
        }

        foreach (var entityEntry in addedEntities)
        {
            var entity = entityEntry.Entity as TenantTable;
            if (entity != null && !entity.TenantId.HasValue)
            {
                entity.TenantId = TenantId;
            }
        }
    }
}
