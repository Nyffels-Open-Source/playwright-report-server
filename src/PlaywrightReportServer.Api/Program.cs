using System.Reflection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using PlaywrightReportServer.Api.Security;
using PlaywrightReportServer.Infrastructure.Extensions;
using PlaywrightReportServer.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.Load("PlaywrightReportServer.Application")));
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<EndpointSecurityOptions>(builder.Configuration.GetSection(EndpointSecurityOptions.SectionName));

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 500L * 1024 * 1024;
});

if (string.IsNullOrWhiteSpace(builder.Configuration.GetValue<string>("EndpointSecurity:WriteApiKey")))
{
    throw new InvalidOperationException("EndpointSecurity:WriteApiKey must be configured.");
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
    db.Database.Migrate();
}

app.MapOpenApi("/openapi/{documentName}.json");
app.MapScalarApiReference(options =>
{
    options.WithTitle("Playwright Report Server API");
    options.WithOpenApiRoutePattern("/openapi/{documentName}.json");
});

app.UseStaticFiles();

var configuredReportsDir = builder.Configuration.GetValue<string>("ArtifactStorage:ReportsDir") ?? "/data/reports";
var reportsDir = Path.GetFullPath(configuredReportsDir);
Directory.CreateDirectory(reportsDir);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(reportsDir),
    RequestPath = "/reports"
});

app.UseAuthorization();
app.MapControllers();

app.Run();
