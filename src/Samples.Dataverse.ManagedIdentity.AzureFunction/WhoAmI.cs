using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace Samples.Dataverse.ManagedIdentity.AzureFunction;

public class WhoAmI
{
    // Authentication is enforced by the function app's built-in authentication
    // (Easy Auth) before this code runs, so the function itself is anonymous.
    // Easy Auth passes the validated caller to us in X-MS-CLIENT-PRINCIPAL.
    [Function("whoami")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        if (!req.Headers.TryGetValue("X-MS-CLIENT-PRINCIPAL", out var header))
        {
            return new UnauthorizedResult();
        }

        using var principal = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(header.ToString())));

        var claims = principal.RootElement.GetProperty("claims")
            .EnumerateArray()
            .ToDictionary(c => c.GetProperty("typ").GetString()!, c => c.GetProperty("val").GetString(), StringComparer.Ordinal);

        return new OkObjectResult(new
        {
            message = "Authenticated with a Dataverse plug-in managed identity.",
            callerApplicationId = claims.GetValueOrDefault("appid") ?? claims.GetValueOrDefault("azp"),
            tenantId = claims.GetValueOrDefault("http://schemas.microsoft.com/identity/claims/tenantid"),
            callerObjectId = claims.GetValueOrDefault("http://schemas.microsoft.com/identity/claims/objectidentifier"),
        });
    }
}
