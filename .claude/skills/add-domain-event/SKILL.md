---
name: add-domain-event
description: Publish a domain event from one slice and react to it in another using the in-process IEventDispatcher in this VSA template. Use when one feature must notify or trigger behavior in another feature without calling its use cases directly.
---

# Publish an event and subscribe to it

Events are the sanctioned way for slices to talk (architecture rule 1). The dispatcher is in-process and synchronous:
it runs every `IEventHandler<T>` in sequence inside the publisher's request scope (same `DbContext`).
There is **no durability**: if a subscriber throws, the exception reaches the publisher and the request fails.
If you need outbox/retries/queues, that is the point to replace the dispatcher with Wolverine.

## 1. Define the event in the publisher's `Contracts/`

`Contracts/` is the public surface of a slice, so other slices may reference it. A plain record is enough.

```csharp
// Features/Products/Contracts/ProductCreated.cs
namespace VsaTemplate.Features.Products.Contracts;

public record ProductCreated(string Name, decimal Price);
```

## 2. Publish from the handler (after persisting)

```csharp
public class CreateProductHandler(AppDbContext db, IEventDispatcher eventDispatcher)
{
    public async Task<Created<CreateProduct.Response>> Handle(CreateProduct.Request request, CancellationToken ct)
    {
        // ... db.Products.Add(product); await db.SaveChangesAsync(ct);
        await eventDispatcher.Publish(new ProductCreated(product.Name, product.Price), ct);
        // ...
    }
}
```

A published event with no subscribers is a valid no-op.

## 3. Subscribe with an `IEventHandler<T>` inside `Features/`

Never put subscribers in `Common` (a test enforces this: reacting to an event is business logic).
Implementing the interface is all that is needed; `AddEvents()` registers it by assembly scan, `Program.cs` is untouched.
In this repo cross-slice reactions live in `Features/Notifications/`, or in the subscribing slice's own folder.

```csharp
// Features/Notifications/ProductCreatedNotification.cs
using VsaTemplate.Common.Events;
using VsaTemplate.Features.Products.Contracts;

namespace VsaTemplate.Features.Notifications;

public class ProductCreatedNotification(ILogger<ProductCreated> logger) : IEventHandler<ProductCreated>
{
    public Task Handle(ProductCreated @event, CancellationToken ct)
    {
        logger.LogInformation("Product Created {Name} - ${Price}", @event.Name, @event.Price);
        return Task.CompletedTask;
    }
}
```

Note: the subscriber's class name must NOT end in `Handler` unless you intend it to be registered as a use-case handler too
(`AddUseCaseHandlers` matches the `Handler` suffix). The convention here is `<Event>Notification` or `On<Event>`.

## 4. Test

Unit-test the publisher with a mocked dispatcher; unit-test the subscriber by calling `Handle` directly;
see `tests/VsaTemplate.Tests/Common/EventDispatcherTests.cs` for dispatcher behavior.

```csharp
var dispatcher = new Mock<IEventDispatcher>();
var handler = new CreateProductHandler(db, dispatcher.Object);
await handler.Handle(new CreateProduct.Request("Teclado", 99m), CancellationToken.None);
dispatcher.Verify(d => d.Publish(It.IsAny<ProductCreated>(), It.IsAny<CancellationToken>()), Times.Once);
```

Run `dotnet test`: `ArchitectureTests` confirm that the subscriber lives under `Features` and that no slice
depends on another slice's use cases.
