using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using VsaTemplate.Common.Persistence;

namespace VsaTemplate.Tests.Common;

#if (IncludeComments)
/// <summary>
/// Crea un AppDbContext real sobre SQLite in-memory con el schema aplicado.
/// Los tests unitarios de handlers usan esto en lugar de mockear el DbContext:
/// se prueba el LINQ y el SQL de verdad. La conexión vive mientras viva el
/// contexto y se libera junto con él.
/// </summary>
#endif
public static class TestDb
{
    public static AppDbContext Create()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new OwnedConnectionDbContext(options, connection);
        db.Database.EnsureCreated();
        return db;
    }

#if (IncludeComments)
    /// <summary>
    /// Crea un AppDbContext sobre una conexión ya abierta, sin tomar
    /// ownership de ella (no la cierra al hacer Dispose). Para tests que
    /// necesitan dos contextos viendo la misma base — ej. un conflicto real
    /// de concurrencia — comparte la conexión entre ambos en vez de crear
    /// dos SQLite in-memory aisladas.
    /// </summary>
#endif
    public static AppDbContext CreateOn(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        return new AppDbContext(options, NullLogger<AppDbContext>.Instance);
    }

    private sealed class OwnedConnectionDbContext(
        DbContextOptions<AppDbContext> options, SqliteConnection connection)
        : AppDbContext(options, NullLogger<AppDbContext>.Instance)
    {
        public override void Dispose()
        {
            base.Dispose();
            connection.Dispose();
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
