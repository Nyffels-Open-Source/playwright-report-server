using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlaywrightReportServer.Application.Abstractions;
using PlaywrightReportServer.Infrastructure.HostedServices;
using PlaywrightReportServer.Infrastructure.Persistence;
using PlaywrightReportServer.Infrastructure.Services;
using PlaywrightReportServer.Infrastructure.Storage;

namespace PlaywrightReportServer.Infrastructure.Extensions;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ArtifactStorageOptions>(configuration.GetSection(ArtifactStorageOptions.SectionName));
        services.Configure<RetentionOptions>(configuration.GetSection(RetentionOptions.SectionName));

        var dbPath = configuration.GetValue<string>("Database:Path") ?? "/data/db.sqlite";
        var dbDir = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrWhiteSpace(dbDir))
        {
            Directory.CreateDirectory(dbDir);
        }

        services.AddDbContext<ReportDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));
        services.AddScoped<IReportDbContext>(sp => sp.GetRequiredService<ReportDbContext>());

        services.AddSingleton<IArtifactStorage, ArtifactStorage>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddHostedService<RetentionHostedService>();

        return services;
    }
}
