using LilyWhiteMap.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LilyWhiteMap.Api.Data;

public sealed class LilyWhiteMapDbContext(DbContextOptions<LilyWhiteMapDbContext> options)
    : DbContext(options)
{
    public DbSet<PlayerEntity> Players => Set<PlayerEntity>();

    public DbSet<VisitLog> VisitLogs => Set<VisitLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlayerEntity>().HasKey(player => player.Id);
        modelBuilder.Entity<PlayerEntity>().HasIndex(player => player.Name);

        modelBuilder.Entity<VisitLog>(entity =>
        {
            entity.ToTable("visit_logs");
            entity.HasKey(v => v.Id);
            entity.Property(v => v.Path).HasMaxLength(512).IsRequired();
            entity.Property(v => v.QueryString).HasMaxLength(1024);
            entity.Property(v => v.IpAddress).HasMaxLength(64);
            entity.Property(v => v.UserAgent).HasMaxLength(512);
            entity.Property(v => v.Referrer).HasMaxLength(1024);
            entity.Property(v => v.VisitorId).HasMaxLength(64).IsRequired();
            entity.HasIndex(v => v.TimestampUtc);
            entity.HasIndex(v => v.VisitorId);
            entity.HasIndex(v => v.Path);
            entity.HasIndex(v => v.IsSuspicious);
        });
    }
}
