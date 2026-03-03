using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlaywrightReportServer.Api.Contracts;
using PlaywrightReportServer.Api.Security;
using PlaywrightReportServer.Application.Common;
using PlaywrightReportServer.Application.Reports.Commands;
using PlaywrightReportServer.Application.Reports.Queries;
using PlaywrightReportServer.Domain.Enums;

namespace PlaywrightReportServer.Api.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ReportsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [RequestSizeLimit(524_288_000)]
    [Consumes("multipart/form-data")]
    [RequireWriteApiKey]
    [ProducesResponseType(typeof(UploadReportResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [EndpointSummary("Upload a Playwright report artifact")]
    [EndpointDescription("Uploads a Playwright report zip file and stores its metadata as a report entry.")]
    public async Task<ActionResult<UploadReportResponse>> Upload([FromForm] UploadReportRequest request, CancellationToken cancellationToken)
    {
        if (request.Artifact is null || request.Artifact.Length == 0)
        {
            return BadRequest("artifact is required");
        }

        await using var stream = request.Artifact.OpenReadStream();
        try
        {
            var response = await _sender.Send(new UploadReportCommand
            {
                ArtifactStream = stream,
                ArtifactLength = request.Artifact.Length,
                FileName = request.Artifact.FileName,
                Name = request.Name,
                Branch = request.Branch,
                Commit = request.Commit,
                Environment = request.Environment,
                Status = request.Status
            }, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = response.ReportId }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ReportListItemDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List Playwright reports")]
    [EndpointDescription("Returns stored Playwright reports with optional filters for branch, environment, and status.")]
    public async Task<ActionResult<IReadOnlyList<ReportListItemDto>>> GetAll([FromQuery] string? branch, [FromQuery] string? environment, [FromQuery] ReportStatus? status, CancellationToken cancellationToken)
    {
        var reports = await _sender.Send(new GetReportsQuery(branch, environment, status), cancellationToken);
        return Ok(reports);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a Playwright report")]
    [EndpointDescription("Returns a single Playwright report by id, including the URL to open the rendered report.")]
    public async Task<ActionResult<ReportDto>> GetById(string id, CancellationToken cancellationToken)
    {
        try
        {
            var report = await _sender.Send(new GetReportByIdQuery(id), cancellationToken);
            return Ok(report);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    [RequireWriteApiKey]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Delete a Playwright report")]
    [EndpointDescription("Deletes a Playwright report entry and its stored report files.")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteReportCommand(id), cancellationToken);
        return NoContent();
    }
}
