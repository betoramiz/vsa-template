namespace VsaTemplate.Common.Pagination;

#if (IncludeComments)
/// <summary>
/// Envuelve el Response de un caso de uso (un ítem) en una página de
/// resultados. Vive en Common porque es genérico sobre T; el Response
/// anidado del caso de uso sigue describiendo un solo ítem.
/// </summary>
#endif
public record PagedResponse<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
}
