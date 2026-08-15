namespace VsaTemplate.Common.ErrorHandling;

public static class ErrorHandlingRegistration
{
#if (IncludeComments)
    /// <summary>
    /// Registra GlobalExceptionHandler como el manejador de excepciones no
    /// controladas y AddProblemDetails() para que toda respuesta de error
    /// (esta y las de TypedResults.NotFound/ValidationProblem) tenga la
    /// forma RFC 9457 estándar. En Development agrega el detalle de la
    /// excepción a la respuesta; en el resto de entornos, solo el mensaje
    /// genérico — el detalle completo siempre queda en el log.
    /// </summary>
#endif
    public static IServiceCollection AddGlobalErrorHandling(this IServiceCollection services, IHostEnvironment environment) =>
        services
            .AddExceptionHandler<GlobalExceptionHandler>()
            .AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                    if (environment.IsDevelopment() && context.Exception is not null)
                        context.ProblemDetails.Extensions["exception"] = context.Exception.ToString();
                };
            });

    public static WebApplication UseGlobalErrorHandling(this WebApplication app)
    {
        app.UseExceptionHandler();
        return app;
    }
}
