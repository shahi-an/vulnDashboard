using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using VulnTrack.Application.Common.Exceptions;

namespace VulnTrack.Api.Middleware;

internal sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning(ex, "Validation failure on {Path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { errors }));
        }
        catch (NotFoundException ex)
        {
            logger.LogInformation(ex, "Not found on {Path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = ex.Message }));
        }
        catch (ForbiddenAccessException ex)
        {
            logger.LogWarning(ex, "Forbidden access on {Path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = ex.Message }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception on {Path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var isDev = context.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() ?? false;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = "An unexpected error occurred.",
                traceId = context.TraceIdentifier,
                detail = isDev ? ex.ToString() : null
            }));
        }
    }
}
