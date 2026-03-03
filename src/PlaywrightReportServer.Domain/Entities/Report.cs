using PlaywrightReportServer.Domain.Enums;

namespace PlaywrightReportServer.Domain.Entities;

public class Report
{
    public string Id { get; set; } = null!;
    public string? Name { get; set; }
    public string? Branch { get; set; }
    public string? CommitSha { get; set; }
    public string? Environment { get; set; }
    public ReportStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string ReportRootRelativePath { get; set; } = null!;
}
