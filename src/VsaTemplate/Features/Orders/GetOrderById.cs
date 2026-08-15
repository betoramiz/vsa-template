using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using VsaTemplate.Common.Persistence;

namespace VsaTemplate.Features.Orders;

public static class GetOrderById
{
    public record Response(Guid Id, Guid ProductId, int Quantity, decimal Total, DateTimeOffset CreatedAt);
}

public class GetOrderByIdHandler(AppDbContext db)
{
    public async Task<Results<Ok<GetOrderById.Response>, NotFound>> Handle(Guid id, CancellationToken ct)
    {
        var response = await db.Orders
            .AsNoTracking()
            .Where(o => o.Id == id)
            .Select(o => new GetOrderById.Response(o.Id, o.ProductId, o.Quantity, o.Total, o.CreatedAt))
            .FirstOrDefaultAsync(ct);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }
}
