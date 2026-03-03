using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace PlaywrightReportServer.Api.Security;

[AttributeUsage(AttributeTargets.Method)]
public sealed class RequireWriteApiKeyAttribute : Attribute, IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var options = context.HttpContext.RequestServices.GetRequiredService<IOptions<EndpointSecurityOptions>>().Value;

        if (string.IsNullOrWhiteSpace(options.WriteApiKey))
        {
            throw new InvalidOperationException("EndpointSecurity:WriteApiKey must be configured.");
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(options.HeaderName, out var providedValues))
        {
            context.Result = new UnauthorizedObjectResult(new { error = "missing api key" });
            return Task.CompletedTask;
        }

        var provided = providedValues.ToString();
        var expectedBytes = Encoding.UTF8.GetBytes(options.WriteApiKey);
        var providedBytes = Encoding.UTF8.GetBytes(provided);
        var isValid = CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);

        if (!isValid)
        {
            context.Result = new UnauthorizedObjectResult(new { error = "invalid api key" });
        }

        return Task.CompletedTask;
    }
}
