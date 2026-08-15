using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using VsaTemplate.Common.Persistence;

namespace VsaTemplate.Features.Products;

public class DeleteProductHandler(AppDbContext db)
{
    public async Task<Results<NoContent, NotFound>> Handle(Guid id, CancellationToken ct)
    {
        var deleted = await db.Products.Where(p => p.Id == id).ExecuteDeleteAsync(ct);

        return deleted == 0 ? TypedResults.NotFound() : TypedResults.NoContent();
    }
}