---
name: add-feature-slice
description: Create a brand-new feature slice (folder under src/VsaTemplate/Features/) in this VSA template, with endpoints, optional domain entity, contracts, and policies. Use when asked to add a new feature/module/bounded area rather than a use case in an existing one.
---

# Add a new feature slice

A slice is a folder under `src/VsaTemplate/Features/<Feature>/`. Everything is discovered by
reflection/convention, so **`Program.cs` is never touched**.

```
Features/Invoices/
  Endpoints.cs          ← routes only (implements IFeatureEndpoints)
  CreateInvoice.cs      ← one file per use case (see the add-use-case skill)
  Policies.cs           ← optional: authorization policies of this slice
  Domain/               ← optional: entities + EF configuration (PUBLIC to other slices)
    Invoice.cs
    InvoiceConfiguration.cs
  Contracts/            ← optional: events this slice publishes (PUBLIC)
    InvoiceIssued.cs
```

## Visibility rules (enforced by `ArchitectureTests`)

- Public surface of a slice = `Domain/` + `Contracts/`. Use cases (types directly in `VsaTemplate.Features.<Feature>`) are private.
- A slice must not use another slice's use cases. Talk through events, or query the other slice's entities via `AppDbContext`
  (`PlaceOrderHandler` reads `Product` this way).
- `Common` must never depend on slice use cases.

## Steps

1. **Endpoints** — class must be named `Endpoints` (the namespace distinguishes slices) and implement `IFeatureEndpoints`;
   otherwise its routes silently do not exist (a test guards this).

```csharp
namespace VsaTemplate.Features.Invoices;

public sealed class Endpoints : IFeatureEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/invoices").WithTags("Invoices");

        group.MapPost("/", (CreateInvoice.Request request, CreateInvoiceHandler handler, CancellationToken ct)
                => handler.Handle(request, ct))
            .WithName(nameof(CreateInvoice))
            .WithValidation<CreateInvoice.Request>();
    }
}
```

2. **Entity (only if it persists data)** — add the entity and, only when EF conventions are not enough, an
   `IEntityTypeConfiguration<T>` next to it. `AppDbContext` applies it via `ApplyConfigurationsFromAssembly`.
   Then register the `DbSet` in `Common/Persistence/AppDbContext.cs`.

```csharp
public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.Property(i => i.Number).HasMaxLength(50);
        builder.Property(i => i.Total).HasPrecision(18, 2);
    }
}
```

   Then create a migration (see the add-ef-migration skill).

3. **Policies (only if the slice needs authorization)** — discovered via `ISlicePolicies`:

```csharp
public sealed class Policies : ISlicePolicies
{
    public const string VoidInvoices = "Invoices.Void";

    public static void Configure(AuthorizationOptions options) =>
        options.AddPolicy(VoidInvoices, policy => policy.RequireRole("Admin"));
}
```

4. **Use cases** — follow the add-use-case skill. Events: see add-domain-event.

5. **Template packaging** — this repo is also a `dotnet new` template. If the slice is a *sample* (not meant to ship by default),
   add its paths to the `(!IncludeSampleFeature)` exclude list in `.template.config/template.json`
   (like `Features/Products/**` and its tests) and wrap sample-only code in `#if (IncludeSampleFeature)`
   (see the `DbSet`s in `AppDbContext.cs`). Real slices need no change there.

## Verify

```bash
dotnet test   # ArchitectureTests re-discover the new slice automatically
```
