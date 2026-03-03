namespace PlaywrightReportServer.Application.Abstractions;

public interface IArtifactStorage
{
    Task<StoredReportResult> StoreAndExtractAsync(StoreReportRequest request, CancellationToken cancellationToken);
    Task DeleteReportFilesAsync(string reportId, CancellationToken cancellationToken);
    string GetReportIndexUrl(string reportId);
}

public sealed record StoreReportRequest(string ReportId, Stream ZipStream, long ZipLength);

public sealed record StoredReportResult(long ExtractedBytes, int ExtractedFiles, string ReportRootRelativePath);
