using Microsoft.EntityFrameworkCore;

namespace VsaTemplate.Common.Persistence;

public static class AppDbContextRegistration
{
    public static IServiceCollection AddAppDbContext(this IServiceCollection services, IConfiguration configuration) =>
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default") ?? "Data Source=vsatemplate.db"));

#if (IncludeComments)
    /// <summary>
    /// Aplica las migraciones pendientes al arrancar (Common/Persistence/Migrations).
    /// Solo en desarrollo. Nueva migración:
    ///   dotnet ef migrations add &lt;Nombre&gt; --project src/VsaTemplate -o Common/Persistence/Migrations
    /// </summary>
#endif
    public static WebApplication ApplyPendingMigrations(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var pendingMigrations = scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.GetPendingMigrations();
            if (pendingMigrations.Any())
            {
                scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
            }
        }

        return app;
    }
}
