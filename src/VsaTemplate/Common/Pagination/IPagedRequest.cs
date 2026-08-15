namespace VsaTemplate.Common.Pagination;

#if (IncludeComments)
/// <summary>
/// Contrato de un request paginado. Es una interfaz, no una clase base: los
/// Request de cada caso de uso siguen siendo records anidados (regla de
/// ArchitectureTests), solo agregan ": IPagedRequest" para reusar validación
/// y la mecánica de Skip/Take de <see cref="PaginationExtensions"/>.
/// </summary>
#endif
public interface IPagedRequest
{
    int PageNumber { get; }
    int PageSize { get; }
}
