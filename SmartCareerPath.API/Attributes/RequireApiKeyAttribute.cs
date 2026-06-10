using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SmartCareerPath.API.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        private const string ApiKeyHeader = "X-Api-Key";

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var config = context.HttpContext.RequestServices
                .GetRequiredService<IConfiguration>();


            var validKey = config["ApiKeys:AiService"];
            if (string.IsNullOrWhiteSpace(validKey))
                throw new InvalidOperationException(
                    "AI service API key is not configured. Set ApiKeys:AiService.");

            if (!context.HttpContext.Request.Headers
                .TryGetValue(ApiKeyHeader, out var extractedKey))
            {
                context.Result = new UnauthorizedObjectResult(
                    new { error = "API key missing." });
                return;
            }

            if (!string.Equals(extractedKey, validKey, StringComparison.Ordinal))
            {
                context.Result = new UnauthorizedObjectResult(
                    new { error = "Invalid API key." });
                return;
            }

            await next();
        }
    }
}
