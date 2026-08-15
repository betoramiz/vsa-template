using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace VsaTemplate.Common.ErrorHandling;

#if (IncludeComments)
/// <summary>
/// Red de seguridad para excepciones no controladas. Los casos esperables
/// (recurso no encontrado, validación) ya se resuelven en el propio handler
/// con TypedResults o el filtro de validación — nunca lanzando excepciones —
/// así que llegar acá siempre es un bug o una falla de infraestructura
/// (DB caída, etc.). Registra el error completo en el log y responde con un
/// ProblemDetails genérico, sin exponer el stack trace al cliente.
/// </summary>
#endif
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception while processing {Method} {Path}",
            httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            },
        });
    }
}
