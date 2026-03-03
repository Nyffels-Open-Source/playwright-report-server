using PlaywrightReportServer.Application.Abstractions;

namespace PlaywrightReportServer.Infrastructure.Services;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
