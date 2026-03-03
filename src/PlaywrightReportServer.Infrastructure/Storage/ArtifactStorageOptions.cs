namespace PlaywrightReportServer.Infrastructure.Storage;

public sealed class ArtifactStorageOptions
{
    public const string SectionName = "ArtifactStorage";

    public string DataDir { get; set; } = "/data";
    public string ReportsDir { get; set; } = "/data/reports";
    public string UploadsDir { get; set; } = "/data/uploads";
    public int MaxUploadMb { get; set; } = 500;
    public int MaxFiles { get; set; } = 20000;
    public int MaxExtractedMb { get; set; } = 2000;
}
