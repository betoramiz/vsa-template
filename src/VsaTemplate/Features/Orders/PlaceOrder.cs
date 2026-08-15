using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using VsaTemplate.Common.Events;
using VsaTemplate.Common.Persistence;
using VsaTemplate.Features.Orders.Contracts;
using VsaTemplate.Features.Orders.Domain;

namespace VsaTemplate.Features.Orders;

#if (IncludeComments)
// Estilo B: los contratos (request/response) se agrupan en una clase estática
// con el nombre del caso de uso, y la lógica vive en una clase de instancia
// registrada en DI (ver Program.cs).
#endif
public static class PlaceOrder
{
    public record Request(Guid ProductId, int Quantity);
    public record Response(Guid Id, decimal Total, DateTimeOffset CreatedAt);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Quantity).GreaterThan(0);
        }
    }
}

#if (IncludeComments)
// TimeProvider es un ejemplo de dependencia "de frontera": en producción llega
// TimeProvider.System; los tests pasan un reloj fijo para verificar CreatedAt
// (ver tests/Features/Orders/PlaceOrderTests.cs).
#endif
public class PlaceOrderHandler(AppDbContext db, TimeProvider clock, IEventDispatcher events)
{
    public async Task<Results<Created<PlaceOrder.Response>, NotFound<string>, Conflict<string>>> Handle(
        PlaceOrder.Request request, CancellationToken ct)
    {
        var product = await db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, ct);

        if (product is null)
            return TypedResults.NotFound("El producto no existe.");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Quantity = request.Quantity,
            Total = product.Price * request.Quantity,
            CreatedAt = clock.GetUtcNow(),
        };

        db.Orders.Add(order);
        var saveResults = await db.TrySaveChangesAsync(ct);
        if (saveResults != SaveResult.Success)
            return TypedResults.Conflict("No se pudo registar la orden, intente nuevamente.");

#if (IncludeComments)
        // In-process y sin durabilidad: si un suscriptor falla, el request
        // falla; si necesitas garantías, este es el punto donde entra Wolverine.
#endif
        await events.Publish(new OrderPlaced(order.Id, order.ProductId, order.Quantity, order.Total), ct);

        return TypedResults.Created(
            $"/orders/{order.Id}",
            new PlaceOrder.Response(order.Id, order.Total, order.CreatedAt));
    }
}
