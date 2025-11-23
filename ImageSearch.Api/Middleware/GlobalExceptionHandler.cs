using ImageSearch.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ImageSearch.Api.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception occurred");

            var (statusCode, title, detail) = exception switch
            {
                RateLimitExceededException => (StatusCodes.Status429TooManyRequests, "Rate Limit Exceeded",
                    exception.Message),
                UnsplashApiException => (StatusCodes.Status503ServiceUnavailable, "Service Unavailable",
                    "Image search service unavailable, please try again after some time"),
                OperationCanceledException => (499, "Request Cancelled", "The request was cancelled"),
                ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request", exception.Message),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred")
            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
    }
}
