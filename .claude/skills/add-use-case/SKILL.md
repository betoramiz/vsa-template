---
name: add-use-case
description: Add a new use case (endpoint + handler + validator + tests) to an existing feature slice in this VSA template. Use when asked to add an endpoint, command, query, or "caso de uso" under src/VsaTemplate/Features/<Feature>/.
---

# Add a use case to an existing slice

A use case is **one file** plus **one mapping line**. Nothing is registered by hand:
`AddUseCaseHandlers()` (Scrutor) registers any public class named `*Handler`, and
`AddValidatorsFromAssembly` finds validators.

## 1. Create `Features/<Feature>/<UseCase>.cs`

A static class named after the use case holds the contracts; a sibling `<UseCase>Handler`
class holds the logic. Nested public types must be named exactly `Request`, `Response`
or `Validator` (enforced by `ArchitectureTests`; they also name the generated TypeScript client types).

```csharp
// Features/Products/CreateProduct.cs
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
        var product = new Product { Id = Guid.NewGuid(), Name = request.Name.Trim(), Price = request.Price };
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
        await eventDispatcher.Publish(new ProductCreated(product.Name, product.Price), ct);
        return TypedResults.Created($"/products/{product.Id}",
            new CreateProduct.Response(product.Id, product.Name, product.Price));
    }
}
```

Rules of thumb:
- Return `TypedResults` / `Results<...>`: they generate the OpenAPI contract with no attributes.
  Example with several outcomes: `Results<Created<PlaceOrder.Response>, NotFound<string>, Conflict<string>>`.
- Queries need no `Request`/`Validator`: `GetProductById` only has a `Response` and `Handle(Guid id, ...)`.
- Use `db.TrySaveChangesAsync(ct)` (returns `SaveResult`) when you must answer `409` on concurrency/duplicate-key; plain `SaveChangesAsync` otherwise.
- No interface for the handler. Inject only boundary abstractions (`TimeProvider`, `IEventDispatcher`).
- Never reference another slice's use cases (only its `Domain/` and `Contracts/`).

## 2. Add one line to the slice's `Endpoints.cs`

Endpoints stay dumb: route → handler. `.WithName(nameof(...))` becomes the TypeScript method name;
add `.WithValidation<T>()` only if the use case has a `Validator`.

```csharp
group.MapPost("/", (CreateProduct.Request request, CreateProductHandler handler, CancellationToken ct)
        => handler.Handle(request, ct))
    .WithName(nameof(CreateProduct))
    .WithValidation<CreateProduct.Request>();

group.MapDelete("/{id:guid}", (Guid id, DeleteProductHandler handler, CancellationToken ct)
        => handler.Handle(id, ct))
    .WithName("DeleteProduct")
    .RequireAuthorization(Policies.DeleteProducts);   // policy declared in the slice's Policies.cs
```

For paged lists, bind with `[AsParameters] GetProducts.Request` and implement `IPagedRequest` (see `GetProducts.cs`).

## 3. Tests (`tests/VsaTemplate.Tests/`)

Handler unit test: build the handler with `new` and a real SQLite in-memory DB (`TestDb.Create()`), no repository mocks.
Validator test: no DB, no HTTP.

```csharp
[Fact]
public async Task Creates_the_product_when_the_data_is_valid()
{
    await using var db = TestDb.Create();
    var handler = new CreateProductHandler(db, new Mock<IEventDispatcher>().Object);

    var result = await handler.Handle(new CreateProduct.Request("Teclado", 99m), CancellationToken.None);

    Assert.Equal("Teclado", result.Value!.Name);
    Assert.Single(db.Products);
}

[Fact]
public void The_validator_rejects_the_empty_name()
{
    var result = new CreateProduct.Validator().Validate(new CreateProduct.Request("   ", 99m));

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == "Name");   // keys are PascalCase
}
```

Integration test (HTTP, `ApiFactory` + `IClassFixture`): `client.PostAsJsonAsync("/products", new { name = "Monitor", price = 300m })`.
For protected routes use `TestJwt.CreateToken(roles: ["Admin"])` as a Bearer token. See `Integration/ProductsApiTests.cs`.

## 4. Verify

```bash
dotnet test
```

`ArchitectureTests` will fail if the handler is not public/`*Handler`, a nested type has the wrong name,
or a slice depends on another slice's use cases.
