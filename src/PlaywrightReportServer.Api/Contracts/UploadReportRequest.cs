using PlaywrightReportServer.Domain.Enums;

namespace PlaywrightReportServer.Api.Contracts;

public sealed class UploadReportRequest
{
    public IFormFile Artifact { get; set; } = null!;
    public string? Name { get; set; }
    public string? Branch { get; set; }
    public string? Commit { get; set; }
    public string? Environment { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Unknown;
}
