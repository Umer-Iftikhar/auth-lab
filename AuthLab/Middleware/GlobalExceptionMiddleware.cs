namespace AuthLab.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Call the next middleware in the pipeline
                await _next(context);
            }
            catch (Exception ex) // Catch any unhandled exceptions
            {
                // Log the exception details using the logger
                _logger.LogCritical(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);


                if (context.Response.HasStarted) // Check if the response has already started, if so, we can't modify it
                {
                    return;
                }

                // Set the response status code to 500 (Internal Server Error) and redirect to a custom error page  
                context.Response.StatusCode = 500;

                // Return a JSON response with error details
                context.Response.ContentType = "application/json";  // Set the content type to JSON
                await context.Response.WriteAsJsonAsync(new        // Write a JSON response with a generic error message 
                {
                    error = "An unexpected error occurred. Please try again later."
                });
            }
        }
    }
}
