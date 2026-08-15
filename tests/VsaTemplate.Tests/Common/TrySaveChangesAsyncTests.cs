using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VsaTemplate.Common.Persistence;
using VsaTemplate.Features.Products.Domain;

namespace VsaTemplate.Tests.Common;

public class TrySaveChangesAsyncTests
{
    [Fact]
    public async Task Returns_success_for_a_normal_insert()
    {
        await using var db = TestDb.Create();

        db.Products.Add(new Product { Id = Guid.NewGuid(), Name = "Teclado", Price = 50m });

        Assert.Equal(SaveResult.Success, await db.TrySaveChangesAsync());
    }

    [Fact]
    public async Task Returns_duplicate_key_when_two_entities_share_the_same_primary_key()
    {
        await using var db = TestDb.Create();
        var id = Guid.NewGuid();

        db.Products.Add(new Product { Id = id, Name = "Teclado", Price = 50m });
        await db.TrySaveChangesAsync();

#if (IncludeComments)
        // Un segundo contexto ignora el tracking del primero: puede intentar
        // insertar la misma Id sin que EF lo prevenga localmente, y choca
        // contra la primary key en la base — el mismo camino que tomaría una
        // condición de carrera real entre dos requests.
#endif
        await using var db2 = TestDb.CreateOn(db.Database.GetDbConnection() as SqliteConnection
            ?? throw new InvalidOperationException("Expected a SqliteConnection."));
        db2.Products.Add(new Product { Id = id, Name = "Mouse", Price = 20m });

        Assert.Equal(SaveResult.DuplicateKey, await db2.TrySaveChangesAsync());
    }

    [Fact]
    public async Task Returns_concurrency_conflict_when_the_row_changed_since_it_was_loaded()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        await using var seed = TestDb.CreateOn(connection);
        await seed.Database.EnsureCreatedAsync();
        var id = Guid.NewGuid();
        seed.Products.Add(new Product { Id = id, Name = "Teclado", Price = 50m });
        await seed.TrySaveChangesAsync();

        await using var db1 = TestDb.CreateOn(connection);
        var fromDb1 = await db1.Products.SingleAsync(p => p.Id == id);

        await using var db2 = TestDb.CreateOn(connection);
        var fromDb2 = await db2.Products.SingleAsync(p => p.Id == id);

        fromDb1.Price = 60m;
        Assert.Equal(SaveResult.Success, await db1.TrySaveChangesAsync());

        fromDb2.Price = 70m;
        Assert.Equal(SaveResult.ConcurrencyConflict, await db2.TrySaveChangesAsync());

        await connection.DisposeAsync();
    }
}
