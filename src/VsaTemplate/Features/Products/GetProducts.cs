using Microsoft.EntityFrameworkCore;
using VsaTemplate.Common.Pagination;
using VsaTemplate.Common.Persistence;

namespace VsaTemplate.Features.Products;

public static class GetProducts
{
    public record Request(int PageNumber = 1, int PageSize = 20) : IPagedRequest;
    public record Response(Guid Id, string Name, decimal Price);

    public class Validator : PagedRequestValidator<Request>;
}

public class GetProductsHandler(AppDbContext db)
{
    public Task<PagedResponse<GetProducts.Response>> Handle(GetProducts.Request request, CancellationToken ct) =>
        db.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new GetProducts.Response(p.Id, p.Name, p.Price))
            .ToPagedResponseAsync(request, ct);
}
