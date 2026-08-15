namespace VsaTemplate.Common.Cors;

#if (IncludeComments)
/// <summary>
/// En Development, cualquier origen puede llamar a la API: el frontend local
/// suele correr en un puerto distinto (o cambiar de puerto entre corridas) y
/// no vale la pena mantenerlo sincronizado con una lista mientras se
/// desarrolla. En el resto de los entornos, solo los orígenes listados en
/// "Cors:AllowedOrigins" — que arranca vacía a propósito (fail-safe: sin
/// configurar, no se permite ningún origen en producción).
/// </summary>
#endif
public static class CorsRegistration
{
    private const string ConfigSection = "Cors";

    public static IServiceCollection AddAppCors(this IServiceCollection services, IHostEnvironment environment, IConfiguration configuration) =>
        services.AddCors(options =>
            options.AddDefaultPolicy(policy =>
            {
                if (environment.IsDevelopment())
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                    return;
                }

                var allowedOrigins = configuration.GetSection($"{ConfigSection}:AllowedOrigins").Get<string[]>() ?? [];
                policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();
            }));
}
