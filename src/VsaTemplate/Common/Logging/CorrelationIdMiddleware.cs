using Serilog.Context;

namespace VsaTemplate.Common.Logging;

#if (IncludeComments)
/// <summary>
/// Reusa HttpContext.TraceIdentifier como correlation ID — es el mismo
/// "traceId" que ya expone GlobalExceptionHandler en cada ProblemDetails de
/// error (Common/ErrorHandling), así que un solo ID conecta lo que ve el
/// cliente con lo que aparece en los logs, sin generar un ID paralelo.
/// Lo mete en el LogContext de Serilog (todo log dentro del request lo
/// hereda) y lo devuelve como header para que el cliente pueda reportarlo.
/// </summary>
#endif
public static class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-Id";

    public static WebApplication UseCorrelationId(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            context.Response.Headers[HeaderName] = context.TraceIdentifier;

            using (LogContext.PushProperty("CorrelationId", context.TraceIdentifier))
                await next();
        });

        return app;
    }
}
