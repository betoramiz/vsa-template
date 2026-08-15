using Microsoft.EntityFrameworkCore;

namespace VsaTemplate.Common.Pagination;

public static class PaginationExtensions
{
#if (IncludeComments)
    /// <summary>
    /// Aplica Skip/Take según el request y cuenta el total, en dos queries
    /// contra la misma IQueryable (el caller ya aplicó su Where/OrderBy).
    /// </summary>
#endif
    public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
        this IQueryable<T> query, IPagedRequest request, CancellationToken ct)
    {
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PagedResponse<T>(items, request.PageNumber, request.PageSize, totalCount);
    }
}
