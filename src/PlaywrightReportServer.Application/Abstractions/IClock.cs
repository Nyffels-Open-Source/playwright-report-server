namespace PlaywrightReportServer.Application.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
