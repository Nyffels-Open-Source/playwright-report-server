using MediatR;
using PlaywrightReportServer.Domain.Enums;

namespace PlaywrightReportServer.Application.Reports.Commands;

public sealed class UploadReportCommand : IRequest<UploadReportResponse>
{
    public Stream ArtifactStream { get; init; } = null!;
    public long ArtifactLength { get; init; }
    public string FileName { get; init; } = null!;
    public string? Name { get; init; }
    public string? Branch { get; init; }
    public string? Commit { get; init; }
    public string? Environment { get; init; }
    public ReportStatus Status { get; init; }
}

public sealed record UploadReportResponse(string ReportId, string ViewUrl);
