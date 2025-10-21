using AppTrack.Api.Models;
using AppTrack.Application.Contracts.Logging;
using AppTrack.Application.Exceptions;
using System.Net;
using BadRequestException = AppTrack.Application.Exceptions.BadRequestException;
using NotFoundException = AppTrack.Application.Exceptions.NotFoundException;

namespace AppTrack.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAppLogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, IAppLogger<ExceptionMiddleware> logger)
    {
        this._next = next;
        this._logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
    {
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
        CustomProblemDetails problem;

        switch (ex)
        {
            case BadRequestException badRequestException:
                statusCode = HttpStatusCode.BadRequest;
                _logger.LogWarning("Validation error: {Message}", badRequestException.Message);
                problem = new CustomProblemDetails()
                {
                    Title = badRequestException.Message.Trim(),
                    Status = (int)statusCode,
                    Detail = badRequestException.InnerException?.Message,
                    Type = nameof(BadRequestException),
                    Errors = badRequestException.ValidationErrors
                };

                break;
            case NotFoundException notFoundException:
                statusCode = HttpStatusCode.NotFound;
                _logger.LogWarning("Resource not found: {Message}", notFoundException.Message);
                problem = new CustomProblemDetails()
                {
                    Title = notFoundException.Message.Trim(),
                    Status = (int)statusCode,
                    Detail = notFoundException.InnerException?.Message,
                    Type = nameof(NotFoundException),
                };

                break;

            case ConflictException conflictException:
                statusCode = HttpStatusCode.Conflict;
                _logger.LogWarning("Resource conflict: {Message}", conflictException.Message);
                problem = new CustomProblemDetails()
                {
                    Title = conflictException.Message.Trim(),
                    Status = (int)statusCode,
                    Detail = conflictException.InnerException?.Message,
                    Type = nameof(ConflictException),
                };

                break;
            default:
                _logger.LogError(ex, "Unhandled exception occurred while processing request {Path}", httpContext.Request.Path);
                problem = new CustomProblemDetails()
                {
                    Title = ex.Message.Trim(),
                    Status = (int)statusCode,
                    Detail = ex.StackTrace,
                    Type = nameof(HttpStatusCode.InternalServerError),
                };

                break;
        }
        httpContext.Response.Clear();
        httpContext.Response.StatusCode = (int)statusCode;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problem);

    }
}
