using Serilog;
using Serilog.Formatting.Json;

namespace VsaTemplate.Common.Logging;

#if (IncludeComments)
/// <summary>
/// Serilog reemplaza por completo al logging provider por defecto (todo
/// ILogger&lt;T&gt; existente, como el de GlobalExceptionHandler, pasa por acá
/// sin cambios). El nivel mínimo y los overrides por categoría se leen de la
/// sección "Serilog" de la configuración (ReadFrom.Configuration) — mismo
/// patrón de JwtOptions/RateLimitingOptions/Cors. Los sinks se deciden en
/// código, no en configuración: consola en texto legible en Development,
/// consola en JSON estructurado en el resto de los entornos. Un sink real de
/// producción (Seq, Elasticsearch, Application Insights, etc.) es un punto
/// de extensión — agregarlo acá.
/// </summary>
#endif
public static class LoggingRegistration
{
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext();

            if (context.HostingEnvironment.IsDevelopment())
                configuration.WriteTo.Console();
            else
                configuration.WriteTo.Console(new JsonFormatter());
        }, preserveStaticLogger: true);
#if (IncludeComments)
        // preserveStaticLogger: no tocamos el Log.Logger estático (compartido
        // por proceso) — cada host resuelve el suyo vía DI. Necesario para que
        // WebApplicationFactory pueda levantar varios hosts en paralelo en los
        // tests sin pisarse entre sí. Efecto colateral: cualquier cosa que use
        // el Log.Logger estático por default (como UseSerilogRequestLogging())
        // debe recibir el logger configurado explícitamente — ver
        // UseAppRequestLogging().
#endif

        return builder;
    }

#if (IncludeComments)
    /// <summary>
    /// Una línea por request (método, path, status, duración), excluyendo
    /// /health/*: un orquestador les pega cada pocos segundos y son ruido, no
    /// señal (ver Common/HealthChecks). Le pasa el logger configurado
    /// explícitamente porque AddSerilogLogging usa preserveStaticLogger:
    /// true, así que el Log.Logger estático que UseSerilogRequestLogging()
    /// usaría por default no está configurado.
    /// </summary>
#endif
    public static WebApplication UseAppRequestLogging(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<Serilog.ILogger>();

        app.UseWhen(
            context => !context.Request.Path.StartsWithSegments("/health"),
            branch => branch.UseSerilogRequestLogging(options => options.Logger = logger));

        return app;
    }
}
