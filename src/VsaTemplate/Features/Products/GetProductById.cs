using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using VsaTemplate.Common.Persistence;

namespace VsaTemplate.Features.Products;

public static class GetProductById
{
    public record Response(Guid Id, string Name, decimal Price);
}

public class GetProductByIdHandler(AppDbContext db)
{
    public async Task<Results<Ok<GetProductById.Response>, NotFound>> Handle(Guid id, CancellationToken ct)
    {
        var response = await db.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new GetProductById.Response(p.Id, p.Name, p.Price))
            .FirstOrDefaultAsync(ct);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }
}
