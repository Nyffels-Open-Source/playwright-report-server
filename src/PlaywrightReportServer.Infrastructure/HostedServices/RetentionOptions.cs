namespace PlaywrightReportServer.Infrastructure.HostedServices;

public sealed class RetentionOptions
{
    public const string SectionName = "Retention";

    public int IntervalHours { get; set; } = 6;
    public int RetentionDays { get; set; } = 30;
    public int RetentionMaxReports { get; set; } = 300;
}
