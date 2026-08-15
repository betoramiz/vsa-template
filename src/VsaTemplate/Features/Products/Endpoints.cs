using Microsoft.AspNetCore.Http;
using VsaTemplate.Common.Endpoints;

namespace VsaTemplate.Features.Products;

public sealed class Endpoints : IFeatureEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products").WithTags("Products");

        group.MapPost("/", (CreateProduct.Request request, CreateProductHandler handler, CancellationToken cancellationToken) 
            => handler.Handle(request, cancellationToken))
            .WithName(nameof(CreateProduct))
            .WithValidation<CreateProduct.Request>();

        group.MapGet("/", ([AsParameters] GetProducts.Request request, GetProductsHandler handler, CancellationToken cancellationToken)
            => handler.Handle(request, cancellationToken))
            .WithName(nameof(GetProducts))
            .WithValidation<GetProducts.Request>();

        group.MapGet("/{id:guid}", (Guid id, GetProductByIdHandler handler, CancellationToken cancellationToken)
            => handler.Handle(id, cancellationToken))
            .WithName(nameof(GetProductById));
        
        group.MapDelete("/{id:guid}", (Guid id, DeleteProductHandler handler, CancellationToken cancellationToken)
            => handler.Handle(id, cancellationToken))
            .WithName("DeleteProduct")
            .RequireAuthorization(Policies.DeleteProducts);
    }
}
