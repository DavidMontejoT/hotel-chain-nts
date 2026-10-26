using HotelChain.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace HotelChain.Api.Middleware;

/// <summary>Traduce las excepciones de la aplicación a respuestas ProblemDetails (RFC 7807).</summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var (status, title) = ex switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, ex.Message),
                ConflictException => (StatusCodes.Status409Conflict, ex.Message),
                ValidationException => (StatusCodes.Status400BadRequest, ex.Message),
                _ => (StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado."),
            };

            if (status == StatusCodes.Status500InternalServerError)
                logger.LogError(ex, "Error no controlado procesando {Path}", context.Request.Path);

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Instance = context.Request.Path,
                // Solo exponemos detalles técnicos en desarrollo.
                Detail = env.IsDevelopment() && status == StatusCodes.Status500InternalServerError ? ex.ToString() : null,
            };

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
        }
    }
}
