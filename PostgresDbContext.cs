using System.Data.Common;
using Maestro.Database;
using Microsoft.EntityFrameworkCore;

namespace Maestro;
public class PostgresDbContext : DbContext {

    public DbSet<Models.Videos>? Videos { get; set; }
    public DbSet<Models.VideoSources>? VideoSources { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
        optionsBuilder.UseNpgsql(connectionString!);
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }
}