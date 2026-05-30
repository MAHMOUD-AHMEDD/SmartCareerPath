namespace SmartCareerPath.API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next,
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
                _logger.LogError(ex, "Unhandled exception");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var (statusCode, message) = ex switch
            {
                KeyNotFoundException => (404, ex.Message),
                InvalidOperationException => (400, ex.Message),
                UnauthorizedAccessException => (401, ex.Message),
                _ => (500, "An unexpected error occurred.")
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsJsonAsync(new { error = message, statusCode });
        }
    }
}
