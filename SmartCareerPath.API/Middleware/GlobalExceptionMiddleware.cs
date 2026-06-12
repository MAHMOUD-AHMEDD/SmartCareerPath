namespace SmartCareerPath.API.Middleware
{
    // Full class after the upgrade:
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try { await _next(context); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception — TraceId: {TraceId}",
                    context.TraceIdentifier);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var (statusCode, message) = ex switch
            {
                KeyNotFoundException => (404, ex.Message),
                UnauthorizedAccessException => (401, ex.Message),
                InvalidOperationException => (400, ex.Message),
                ArgumentException => (400, ex.Message),
                // OperationCanceledException: client disconnected — no response to send.
                // Returning 499 is an nginx-only convention and not an HTTP standard code.
                // We log it and return 400; the client will not read the response anyway.
                OperationCanceledException => (400, "The request was cancelled."),
                _ => (500, "An unexpected error occurred.")
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            return context.Response.WriteAsJsonAsync(new
            {
                statusCode,
                error = message,
                traceId = context.TraceIdentifier  // useful for correlating logs
            });
        }
    }
}
