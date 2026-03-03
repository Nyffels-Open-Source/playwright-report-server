using System.IO.Compression;
using Microsoft.Extensions.Options;
using PlaywrightReportServer.Application.Abstractions;

namespace PlaywrightReportServer.Infrastructure.Storage;

public sealed class ArtifactStorage : IArtifactStorage
{
    private readonly ArtifactStorageOptions _options;

    public ArtifactStorage(IOptions<ArtifactStorageOptions> options)
    {
        _options = options.Value;
        Directory.CreateDirectory(_options.DataDir);
        Directory.CreateDirectory(_options.ReportsDir);
        Directory.CreateDirectory(_options.UploadsDir);
    }

    public async Task<StoredReportResult> StoreAndExtractAsync(StoreReportRequest request, CancellationToken cancellationToken)
    {
        var maxUploadBytes = (long)_options.MaxUploadMb * 1024 * 1024;
        if (request.ZipLength > maxUploadBytes)
        {
            throw new InvalidOperationException($"Upload exceeds max size {_options.MaxUploadMb} MB.");
        }

        var zipPath = Path.Combine(_options.UploadsDir, $"{request.ReportId}.zip");
        var reportRoot = Path.Combine(_options.ReportsDir, request.ReportId);
        var reportRootFull = Path.GetFullPath(reportRoot);

        Directory.CreateDirectory(reportRoot);

        try
        {
            await using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await request.ZipStream.CopyToAsync(fs, cancellationToken);
            }

            var maxExtractedBytes = (long)_options.MaxExtractedMb * 1024 * 1024;
            long extractedBytes = 0;
            var fileCount = 0;

            using (var archive = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in archive.Entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        continue;
                    }

                    fileCount++;
                    if (fileCount > _options.MaxFiles)
                    {
                        throw new InvalidOperationException($"Archive contains too many files. Max is {_options.MaxFiles}.");
                    }

                    var destinationPath = Path.GetFullPath(Path.Combine(reportRoot, entry.FullName));
                    if (!destinationPath.StartsWith(reportRootFull + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                        && !string.Equals(destinationPath, reportRootFull, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException("Archive contains invalid paths.");
                    }

                    var destinationDirectory = Path.GetDirectoryName(destinationPath);
                    if (!string.IsNullOrEmpty(destinationDirectory))
                    {
                        Directory.CreateDirectory(destinationDirectory);
                    }

                    await using var entryStream = entry.Open();
                    await using var outputStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);

                    var buffer = new byte[81920];
                    int read;
                    while ((read = await entryStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
                    {
                        extractedBytes += read;
                        if (extractedBytes > maxExtractedBytes)
                        {
                            throw new InvalidOperationException($"Archive exceeds extracted size limit of {_options.MaxExtractedMb} MB.");
                        }

                        await outputStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                    }
                }
            }

            var reportIndex = Path.Combine(reportRoot, "playwright-report", "index.html");
            if (!File.Exists(reportIndex))
            {
                throw new InvalidOperationException("Archive must contain playwright-report/index.html.");
            }

            return new StoredReportResult(
                ExtractedBytes: extractedBytes,
                ExtractedFiles: fileCount,
                ReportRootRelativePath: "playwright-report");
        }
        catch
        {
            if (Directory.Exists(reportRoot))
            {
                Directory.Delete(reportRoot, recursive: true);
            }

            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            throw;
        }
        finally
        {
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }
        }
    }

    public Task DeleteReportFilesAsync(string reportId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var reportRoot = Path.Combine(_options.ReportsDir, reportId);
        var zipPath = Path.Combine(_options.UploadsDir, $"{reportId}.zip");

        if (Directory.Exists(reportRoot))
        {
            Directory.Delete(reportRoot, recursive: true);
        }

        if (File.Exists(zipPath))
        {
            File.Delete(zipPath);
        }

        return Task.CompletedTask;
    }

    public string GetReportIndexUrl(string reportId) => $"/reports/{reportId}/playwright-report/index.html";
}
