using MediatR;
using Microsoft.EntityFrameworkCore;
using PlaywrightReportServer.Application.Abstractions;
using PlaywrightReportServer.Domain.Entities;

namespace PlaywrightReportServer.Application.Reports.Commands;

public sealed class UploadReportCommandHandler : IRequestHandler<UploadReportCommand, UploadReportResponse>
{
    private const long MaxUploadBytes = 500L * 1024 * 1024;
    private readonly IReportDbContext _dbContext;
    private readonly IArtifactStorage _artifactStorage;
    private readonly IClock _clock;

    public UploadReportCommandHandler(IReportDbContext dbContext, IArtifactStorage artifactStorage, IClock clock)
    {
        _dbContext = dbContext;
        _artifactStorage = artifactStorage;
        _clock = clock;
    }

    public async Task<UploadReportResponse> Handle(UploadReportCommand request, CancellationToken cancellationToken)
    {
        if (!request.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only .zip files are supported.");
        }

        if (request.ArtifactLength <= 0)
        {
            throw new InvalidOperationException("Upload is empty.");
        }

        if (request.ArtifactLength > MaxUploadBytes)
        {
            throw new InvalidOperationException("Upload exceeds max size 500 MB.");
        }

        var reportId = Guid.NewGuid().ToString("n");
        var stored = await _artifactStorage.StoreAndExtractAsync(
            new StoreReportRequest(reportId, request.ArtifactStream, request.ArtifactLength),
            cancellationToken);

        var report = new Report
        {
            Id = reportId,
            Name = request.Name,
            Branch = request.Branch,
            CommitSha = request.Commit,
            Environment = request.Environment,
            Status = request.Status,
            CreatedAtUtc = _clock.UtcNow,
            ReportRootRelativePath = stored.ReportRootRelativePath
        };

        await _dbContext.Reports.AddAsync(report, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var viewUrl = _artifactStorage.GetReportIndexUrl(reportId);
        return new UploadReportResponse(reportId, viewUrl);
    }
}
