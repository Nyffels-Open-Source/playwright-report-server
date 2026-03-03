using PlaywrightReportServer.Domain.Enums;

namespace PlaywrightReportServer.Application.Common;

public sealed record ReportListItemDto(
    string Id,
    string? Name,
    string? Branch,
    string? CommitSha,
    string? Environment,
    ReportStatus Status,
    DateTime CreatedAtUtc,
    string ViewUrl);

public sealed record ReportDto(
    string Id,
    string? Name,
    string? Branch,
    string? CommitSha,
    string? Environment,
    ReportStatus Status,
    DateTime CreatedAtUtc,
    string ViewUrl,
    string ReportRootRelativePath);
