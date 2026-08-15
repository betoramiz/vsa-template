using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using VsaTemplate.Common.Events;
using VsaTemplate.Common.Persistence;
using VsaTemplate.Features.Products.Contracts;
using VsaTemplate.Features.Products.Domain;

namespace VsaTemplate.Features.Products;

public static class CreateProduct
{
    public record Request(string Name, decimal Price);
    public record Response(Guid Id, string Name, decimal Price);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Price).GreaterThan(0);
        }
    }
}

public class CreateProductHandler(AppDbContext db, IEventDispatcher eventDispatcher)
{
    public async Task<Created<CreateProduct.Response>> Handle(CreateProduct.Request request, CancellationToken ct)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Price = request.Price,
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
        
        await eventDispatcher.Publish(new ProductCreated(product.Name, product.Price), ct);

        return TypedResults.Created(
            $"/products/{product.Id}",
            new CreateProduct.Response(product.Id, product.Name, product.Price));
    }
}
