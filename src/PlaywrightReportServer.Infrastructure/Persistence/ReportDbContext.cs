using Microsoft.EntityFrameworkCore;
using PlaywrightReportServer.Application.Abstractions;
using PlaywrightReportServer.Domain.Entities;

namespace PlaywrightReportServer.Infrastructure.Persistence;

public sealed class ReportDbContext : DbContext, IReportDbContext
{
    public ReportDbContext(DbContextOptions<ReportDbContext> options) : base(options)
    {
    }

    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.Name).HasMaxLength(256);
            entity.Property(x => x.Branch).HasMaxLength(256);
            entity.Property(x => x.CommitSha).HasMaxLength(128);
            entity.Property(x => x.Environment).HasMaxLength(128);
            entity.Property(x => x.ReportRootRelativePath).HasMaxLength(512).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
        });
    }
}
