using Microsoft.EntityFrameworkCore;
using PlaywrightReportServer.Domain.Entities;

namespace PlaywrightReportServer.Application.Abstractions;

public interface IReportDbContext
{
    DbSet<Report> Reports { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
