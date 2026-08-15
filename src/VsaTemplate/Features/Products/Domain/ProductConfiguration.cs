using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VsaTemplate.Features.Products.Domain;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Name).HasMaxLength(200);
        builder.Property(p => p.Price).HasPrecision(18, 2);
#if (IncludeComments)
        // Concurrency token manejado a mano, no .IsRowVersion(): SQLite no
        // genera valores de rowversion en la DB (a diferencia de SQL Server),
        // así que el valor lo actualiza AppDbContext.TrySaveChangesAsync antes
        // de guardar (Common/Persistence/AppDbContext.cs).
        // --- SQLite (activo) ---
#endif
        builder.Property(p => p.RowVersion).IsConcurrencyToken();

#if (IncludeComments)
        // --- PostgreSQL (para cuando migres el provider) ---
        // Postgres tiene un equivalente nativo mejor: la columna de sistema
        // xmin cambia sola en cada UPDATE, sin que la app tenga que generar
        // ni mantener ningún valor. Al migrar:
        //   1. Comenta la línea de arriba (RowVersion/IsConcurrencyToken).
        //   2. Descomenta esta línea.
        //   3. Borra Product.RowVersion, IHasRowVersion, y el loop que lo
        //      bumpea en AppDbContext.TrySaveChangesAsync — ya no hacen
        //      falta, Postgres resuelve el valor solo.
        //   4. Elimina la migración que agrega la columna RowVersion y
        //      regenera el modelo para el provider nuevo.
        //
        // builder.Property<uint>("xmin").IsRowVersion();
#endif
    }
}
