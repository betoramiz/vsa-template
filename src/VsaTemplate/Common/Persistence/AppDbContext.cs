using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
#if (IncludeSampleFeature)
using VsaTemplate.Features.Orders.Domain;
using VsaTemplate.Features.Products.Domain;
#endif

namespace VsaTemplate.Common.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options, ILogger<AppDbContext> logger) : DbContext(options)
{
#if (IncludeSampleFeature)
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
#endif

#if (IncludeComments)
    // Descubre todas las IEntityTypeConfiguration<> del assembly. Cada
    // configuración vive junto a su entidad, en el Domain/ de su slice
    // (ej. Features/Products/Domain/ProductConfiguration.cs) — agregar una
    // nueva no toca este archivo.
#endif
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

#if (IncludeComments)
    /// <summary>
    /// Punto de entrada recomendado para guardar cambios: distingue las dos
    /// fallas esperables de negocio (conflicto de concurrencia, clave
    /// duplicada) de cualquier otra excepción, que se sigue propagando sin
    /// tocar — esa la resuelve GlobalExceptionHandler como un 500 genérico.
    /// Ambos casos manejados se loguean como Warning (son condiciones
    /// esperables, no bugs) y el resultado queda disponible para que el
    /// caller decida el status code correcto.
    /// </summary>
#endif
    public async Task<SaveResult> TrySaveChangesAsync(CancellationToken ct = default)
    {
#if (IncludeComments)
        // Solo Modified: en Deleted, EF usa el valor ya cargado como
        // predicado WHERE del chequeo optimista — tocarlo acá lo rompería.
        // SQLite-only — ver la nota sobre PostgreSQL más abajo, junto a
        // IsUniqueConstraintViolation: con xmin como concurrency token este
        // loop entero sobra, Postgres genera el valor solo.
#endif
        foreach (var entry in ChangeTracker.Entries<IHasRowVersion>())
            if (entry.State == EntityState.Modified)
                entry.Entity.RowVersion = Guid.NewGuid().ToByteArray();

        try
        {
            await SaveChangesAsync(ct);
            return SaveResult.Success;
        }
        catch (DbUpdateConcurrencyException ex)
        {
#if (IncludeComments)
            // Agnóstico de proveedor: DbUpdateConcurrencyException es de EF
            // Core, no de SQLite — este catch no cambia si migrás a Postgres.
#endif
            logger.LogWarning(ex, "Concurrency conflict while saving changes.");
            return SaveResult.ConcurrencyConflict;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            logger.LogWarning(ex, "Unique constraint violation while saving changes.");
            return SaveResult.DuplicateKey;
        }
    }

#if (IncludeComments)
    // --- SQLite (activo) ---
#endif
    private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
        ex.InnerException is SqliteException inner &&
#if (IncludeComments)
        // SQLITE_CONSTRAINT
#endif
        inner.SqliteErrorCode == 19 &&
        inner.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase);

#if (IncludeComments)
    // --- PostgreSQL (para cuando migres el provider) ---
    // Al cambiar AppDbContextRegistration.cs de UseSqlite a UseNpgsql:
    //   1. Agrega el paquete Npgsql (ya trae PostgresException/PostgresErrorCodes).
    //   2. Comenta el método IsUniqueConstraintViolation de arriba (SQLite) y
    //      descomenta este.
    //   3. Borra el using Microsoft.Data.Sqlite de este archivo si ya no
    //      queda ningún otro uso.
    // Postgres expone un SqlState explícito y documentado (23505 = unique
    // violation) en vez de un código genérico + parseo de mensaje — no hace
    // falta adivinar el texto del error como con SQLite.
    //
    // using Npgsql;
    //
    // private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
    //     ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
#endif
}
