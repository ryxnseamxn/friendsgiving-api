using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Friendsgiving.Api.Infrastructure;

/// <summary>
/// Requires the <c>X-Admin-Key</c> header to match <c>Admin:ApiKey</c>. When no key is configured,
/// admin endpoints are open in Development and refused everywhere else.
/// </summary>
public sealed class AdminKeyAttribute : Attribute, IAuthorizationFilter
{
    public const string HeaderName = "X-Admin-Key";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var services = context.HttpContext.RequestServices;
        var expected = services.GetRequiredService<IConfiguration>()["Admin:ApiKey"];

        if (string.IsNullOrEmpty(expected))
        {
            if (!services.GetRequiredService<IHostEnvironment>().IsDevelopment())
            {
                context.Result = new ObjectResult("Admin access is disabled: Admin:ApiKey is not configured.")
                {
                    StatusCode = StatusCodes.Status503ServiceUnavailable,
                };
            }
            return;
        }

        var provided = context.HttpContext.Request.Headers[HeaderName].ToString();
        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(provided), Encoding.UTF8.GetBytes(expected)))
        {
            context.Result = new UnauthorizedObjectResult("Invalid admin key.");
        }
    }
}
