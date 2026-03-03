using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlaywrightReportServer.Application.Abstractions;
using PlaywrightReportServer.Infrastructure.Persistence;

namespace PlaywrightReportServer.Infrastructure.HostedServices;

public sealed class RetentionHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<RetentionOptions> _options;
    private readonly ILogger<RetentionHostedService> _logger;

    public RetentionHostedService(
        IServiceProvider serviceProvider,
        IOptions<RetentionOptions> options,
        ILogger<RetentionHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRetentionAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Retention cleanup failed.");
            }

            var delay = TimeSpan.FromHours(Math.Max(1, _options.Value.IntervalHours));
            await Task.Delay(delay, stoppingToken);
        }
    }

    private async Task ProcessRetentionAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
        var storage = scope.ServiceProvider.GetRequiredService<IArtifactStorage>();

        var now = DateTime.UtcNow;
        var cutoff = now.AddDays(-Math.Max(1, _options.Value.RetentionDays));

        var reportsOrdered = await db.Reports
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var maxReports = Math.Max(1, _options.Value.RetentionMaxReports);
        var toDelete = reportsOrdered
            .Where((report, idx) => report.CreatedAtUtc < cutoff || idx >= maxReports)
            .ToList();

        if (toDelete.Count == 0)
        {
            return;
        }

        foreach (var report in toDelete)
        {
            db.Reports.Remove(report);
            await storage.DeleteReportFilesAsync(report.Id, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Retention cleanup removed {Count} report(s)", toDelete.Count);
    }
}
