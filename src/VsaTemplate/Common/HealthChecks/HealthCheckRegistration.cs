using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using VsaTemplate.Common.Persistence;

namespace VsaTemplate.Common.HealthChecks;

#if (IncludeComments)
/// <summary>
/// Dos endpoints con semántica distinta, el estándar en Kubernetes/cualquier
/// orquestador:
/// - /health/live: el proceso está vivo y puede responder requests, sin
///   evaluar dependencias. Si falla, el orquestador reinicia la instancia.
/// - /health/ready: además, sus dependencias (hoy, la base de datos) están
///   disponibles. Si falla, el orquestador saca la instancia del balanceador
///   pero NO la reinicia — puede recuperarse sola cuando la dependencia vuelva.
/// </summary>
#endif
public static class HealthCheckRegistration
{
    private const string ReadyTag = "ready";

    public static IServiceCollection AddAppHealthChecks(this IServiceCollection services) =>
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>(tags: [ReadyTag])
            .Services;

    public static WebApplication MapAppHealthChecks(this WebApplication app)
    {
#if (IncludeComments)
        // DisableRateLimiting(): un orquestador pega a estos endpoints con
        // mucha más frecuencia que cualquier cliente real; si compartieran
        // el límite global, las probes fallarían por su propia frecuencia y
        // el orquestador reiniciaría instancias sanas.
#endif
        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false })
            .ExcludeFromDescription()
            .DisableRateLimiting();

        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains(ReadyTag) })
            .ExcludeFromDescription()
            .DisableRateLimiting();

        return app;
    }
}
