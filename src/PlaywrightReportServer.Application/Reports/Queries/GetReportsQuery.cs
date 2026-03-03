using MediatR;
using Microsoft.EntityFrameworkCore;
using PlaywrightReportServer.Application.Abstractions;
using PlaywrightReportServer.Application.Common;
using PlaywrightReportServer.Domain.Enums;

namespace PlaywrightReportServer.Application.Reports.Queries;

public sealed record GetReportsQuery(string? Branch, string? Environment, ReportStatus? Status) : IRequest<IReadOnlyList<ReportListItemDto>>;

public sealed class GetReportsQueryHandler : IRequestHandler<GetReportsQuery, IReadOnlyList<ReportListItemDto>>
{
    private readonly IReportDbContext _dbContext;
    private readonly IArtifactStorage _artifactStorage;

    public GetReportsQueryHandler(IReportDbContext dbContext, IArtifactStorage artifactStorage)
    {
        _dbContext = dbContext;
        _artifactStorage = artifactStorage;
    }

    public async Task<IReadOnlyList<ReportListItemDto>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Reports.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Branch))
        {
            query = query.Where(x => x.Branch == request.Branch);
        }

        if (!string.IsNullOrWhiteSpace(request.Environment))
        {
            query = query.Where(x => x.Environment == request.Environment);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        var reports = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new ReportListItemDto(
                x.Id,
                x.Name,
                x.Branch,
                x.CommitSha,
                x.Environment,
                x.Status,
                x.CreatedAtUtc,
                _artifactStorage.GetReportIndexUrl(x.Id)))
            .ToListAsync(cancellationToken);

        return reports;
    }
}
