using MediatR;
using Microsoft.EntityFrameworkCore;
using PlaywrightReportServer.Application.Abstractions;

namespace PlaywrightReportServer.Application.Reports.Commands;

public sealed record DeleteReportCommand(string ReportId) : IRequest;

public sealed class DeleteReportCommandHandler : IRequestHandler<DeleteReportCommand>
{
    private readonly IReportDbContext _dbContext;
    private readonly IArtifactStorage _artifactStorage;

    public DeleteReportCommandHandler(IReportDbContext dbContext, IArtifactStorage artifactStorage)
    {
        _dbContext = dbContext;
        _artifactStorage = artifactStorage;
    }

    public async Task Handle(DeleteReportCommand request, CancellationToken cancellationToken)
    {
        var report = await _dbContext.Reports.FirstOrDefaultAsync(x => x.Id == request.ReportId, cancellationToken);
        if (report is null)
        {
            return;
        }

        _dbContext.Reports.Remove(report);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _artifactStorage.DeleteReportFilesAsync(request.ReportId, cancellationToken);
    }
}
