using Microsoft.EntityFrameworkCore;

namespace LilyWhiteMap.Api.Data;

public sealed class LilyWhiteMapDbContext(DbContextOptions<LilyWhiteMapDbContext> options)
    : DbContext(options)
{
    public DbSet<PlayerEntity> Players => Set<PlayerEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlayerEntity>().HasKey(player => player.Id);
        modelBuilder.Entity<PlayerEntity>().HasIndex(player => player.Name);
    }
}
