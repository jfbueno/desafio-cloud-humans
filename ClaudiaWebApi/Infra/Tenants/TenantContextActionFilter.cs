using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ClaudiaWebApi.Infra.Tenants;

/// <summary>
/// An action filter that populates the current request's tenant context (<see cref="TenantContext"/>)
/// based on ProjectName and HelpdeskId values found in the action's request body.
/// </summary>
/// <remarks>This filter inspects the first action argument for properties named "ProjectName" and "HelpdeskId".
/// If both are present and valid, it sets the corresponding values on the TenantContext service for the duration of the
/// request. If either value is missing or invalid, the filter short-circuits the request with a 400 Bad Request
/// response.</remarks>
public sealed class TenantContextActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // It's important to note that reflection is not the most performant approach,
        // especially in high-throughput scenarios. 
        // However, for the sake of flexibility and simplicity in this example, we are using it here.
        // In a production scenario, we should consider receiving this information in the HTTP headers.

        var requestBody = context.ActionArguments.Values.FirstOrDefault();

        if (requestBody is null)
        {
            await next();
            return;
        }

        var projectNameProp = requestBody.GetType().GetProperty("ProjectName");
        var helpdeskIdProp = requestBody.GetType().GetProperty("HelpdeskId");

        // Here we're assuming that every request must provide both ProjectName and HelpdeskId
        // to identify the tenant context.
        if (projectNameProp is not null && helpdeskIdProp is not null)
        {
            var projectName = projectNameProp.GetValue(requestBody);
            var helpdeskId = helpdeskIdProp.GetValue(requestBody);

            if (string.IsNullOrEmpty(projectName?.ToString()) || helpdeskId is null)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    Error = "ProjectName and HelpdeskId must be provided"
                });

                return;
            }

            var tenantContext = context.HttpContext.RequestServices
                .GetRequiredService<TenantContext>();

            tenantContext.ProjectName = projectName.ToString()!;
            tenantContext.HelpdeskId = Convert.ToInt32(helpdeskId);
        }

        await next();
    }
}
