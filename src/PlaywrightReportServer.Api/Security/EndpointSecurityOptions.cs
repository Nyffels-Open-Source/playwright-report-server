namespace PlaywrightReportServer.Api.Security;

public sealed class EndpointSecurityOptions
{
    public const string SectionName = "EndpointSecurity";

    public string HeaderName { get; set; } = "X-Api-Key";
    public string WriteApiKey { get; set; } = string.Empty;
}
