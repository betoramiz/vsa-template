namespace VsaTemplate.Common.Persistence;

#if (IncludeComments)
/// <summary>
/// Marca una entidad con un concurrency token de tipo byte[]. SQLite no
/// tiene un tipo rowversion/timestamp que la DB actualice sola (a diferencia
/// de SQL Server): AppDbContext.TrySaveChangesAsync genera un nuevo valor
/// para cada entidad modificada antes de guardar — así el chequeo optimista
/// (WHERE incluye el valor original) sí puede fallar cuando otro proceso ya
/// cambió la fila.
/// </summary>
#endif
public interface IHasRowVersion
{
    byte[] RowVersion { get; set; }
}
