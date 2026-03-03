using MediatR;
using Microsoft.EntityFrameworkCore;
using PlaywrightReportServer.Application.Abstractions;
using PlaywrightReportServer.Application.Common;

namespace PlaywrightReportServer.Application.Reports.Queries;

public sealed record GetReportByIdQuery(string ReportId) : IRequest<ReportDto>;

public sealed class GetReportByIdQueryHandler : IRequestHandler<GetReportByIdQuery, ReportDto>
{
    private readonly IReportDbContext _dbContext;
    private readonly IArtifactStorage _artifactStorage;

    public GetReportByIdQueryHandler(IReportDbContext dbContext, IArtifactStorage artifactStorage)
    {
        _dbContext = dbContext;
        _artifactStorage = artifactStorage;
    }

    public async Task<ReportDto> Handle(GetReportByIdQuery request, CancellationToken cancellationToken)
    {
        var report = await _dbContext.Reports
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ReportId, cancellationToken)
            ?? throw new KeyNotFoundException($"Report '{request.ReportId}' was not found.");

        return new ReportDto(
            report.Id,
            report.Name,
            report.Branch,
            report.CommitSha,
            report.Environment,
            report.Status,
            report.CreatedAtUtc,
            _artifactStorage.GetReportIndexUrl(report.Id),
            report.ReportRootRelativePath);
    }
}
