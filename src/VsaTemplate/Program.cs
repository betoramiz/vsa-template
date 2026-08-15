using FluentValidation;
using VsaTemplate.Common.Auth;
using VsaTemplate.Common.Cors;
using VsaTemplate.Common.Endpoints;
using VsaTemplate.Common.ErrorHandling;
using VsaTemplate.Common.Events;
using VsaTemplate.Common.HealthChecks;
using VsaTemplate.Common.Logging;
using VsaTemplate.Common.OpenApi;
using VsaTemplate.Common.Persistence;
using VsaTemplate.Common.RateLimiting;
using VsaTemplate.Composition;

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilogLogging();

#if (IncludeComments)
// Punto de extensión para un secret store real en producción (Azure Key
// Vault, AWS Secrets Manager, etc.), p. ej. builder.Configuration.AddAzureKeyVault(...).
// No incluido por defecto para no atar el template a una nube — hasta
// entonces, los secretos se resuelven con user-secrets en dev y variables
// de entorno en producción (ver README, "Manejo de secretos").
#endif

builder.Services
    .AddGlobalErrorHandling(builder.Environment)
    .AddAppCors(builder.Environment, builder.Configuration)
    .AddAppHealthChecks()
    .AddAppRateLimiting(builder.Configuration)
    .AddOpenApiDocument()
    .AddAppDbContext(builder.Configuration)
    .AddJwtAuthentication(builder.Configuration)
    .AddSingleton(TimeProvider.System)
    .AddEvents()
    .AddValidatorsFromAssembly(typeof(Program).Assembly)
    .AddUseCaseHandlers();

var app = builder.Build();

app.UseCorrelationId();
app.UseAppRequestLogging();
app.UseGlobalErrorHandling();
app.ApplyPendingMigrations();
app.MapAppHealthChecks();
app.MapOpenApiDocument();
app.UseCors();
app.UseAuthentication();
app.UseAppRateLimiting();
app.UseAuthorization();
app.MapFeatureEndpoints();

app.Run();

#if (IncludeComments)
// Expone Program a los tests de integración (WebApplicationFactory<Program>).
#endif
public partial class Program;
