using Maestro.Auth;
using Microsoft.EntityFrameworkCore;

namespace Maestro.Entities;
public partial class MediaDbContext(
    DbContextOptions<MediaDbContext> options, 
    IUserContextProvider userContextProvider) : DbContext(options) {
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

    private void AddQueryFilter<T> (ModelBuilder modelBuilder) where T: TenantTable {
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
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void SetTenantIdForAddedEntities()
    {
        var addedEntities = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added && e.Entity is TenantTable);

        if(TenantId == null) {
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