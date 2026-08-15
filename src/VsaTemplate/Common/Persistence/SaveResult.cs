namespace VsaTemplate.Common.Persistence;

#if (IncludeComments)
/// <summary>
/// Resultado de AppDbContext.TrySaveChangesAsync(). Solo distingue fallas
/// esperables de negocio (el handler puede reaccionar con un status code
/// específico) de todo lo demás — una excepción no controlada durante el
/// guardado sigue propagándose sin pasar por acá, para que
/// GlobalExceptionHandler siga siendo la única red de seguridad para lo
/// inesperado.
/// </summary>
#endif
public enum SaveResult
{
    Success,
    ConcurrencyConflict,
    DuplicateKey,
}
