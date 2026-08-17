using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Common.Exceptions;

namespace TaskFlow.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, title, detail, errors) = exception switch
        {
            ValidationException valEx => (
                HttpStatusCode.BadRequest,
                "Validation Error",
                valEx.Message,
                valEx.Errors),

            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                "Resource Not Found",
                notFoundEx.Message,
                null),

            ForbiddenException forbiddenEx => (
                HttpStatusCode.Forbidden,
                "Access Forbidden",
                forbiddenEx.Message,
                null),

            ConflictException conflictEx => (
                HttpStatusCode.Conflict,
                "Conflict",
                conflictEx.Message,
                null),

            _ => (
                HttpStatusCode.InternalServerError,
                "Server Error",
                "An unexpected error occurred on the server.",
                null)
        };

        context.Response.StatusCode = (int)statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        if (errors != null && errors.Count > 0)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        var json = JsonSerializer.Serialize(problemDetails);
        return context.Response.WriteAsync(json);
    }
}
