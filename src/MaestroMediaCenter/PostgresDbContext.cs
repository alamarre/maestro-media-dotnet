using System.Data.Common;
using System.Linq.Expressions;
using Maestro.Auth;
using Maestro.Database;
using Maestro.Models;
using Microsoft.EntityFrameworkCore;

namespace Maestro;
public class MediaDbContext(DbContextOptions<MediaDbContext> options, IUserContextProvider userContextProvider) : DbContext(options) {

    public required DbSet<Models.TenantDomains> TenantDomains { get; set; }
    public required DbSet<Models.AccountUsers> AccountUsers { get; set; }
    public required DbSet<Models.AccountEmails> AccountEmails { get; set; }
    public required DbSet<Models.AccountLogins> AccountLogins { get; set; }

    public required DbSet<Models.Profiles> Profiles { get; set; }

    public required DbSet<Models.Videos> Videos { get; set; }
    public required DbSet<Models.VideoSources> VideoSources { get; set; }
    public required DbSet<Models.VideoSourceRoots> VideoSourceRoots { get; set; }

    public required DbSet<Models.WatchProgress> WatchProgress { get; set; }

    public required DbSet<Models.VideoCollections> VideoCollections { get; set; }
    public required DbSet<Models.VideoCollectionItems> VideoCollectionItems { get; set; }

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
        AddQueryFilter<AccountLogins>(modelBuilder);
        AddQueryFilter<AccountEmails>(modelBuilder);
        AddQueryFilter<AccountUsers>(modelBuilder);
        AddQueryFilter<Profiles>(modelBuilder);
        AddQueryFilter<Videos>(modelBuilder);
    }

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