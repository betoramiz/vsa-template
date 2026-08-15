using Microsoft.AspNetCore.Http.HttpResults;
using VsaTemplate.Common.Events;
using VsaTemplate.Features.Orders;
using VsaTemplate.Features.Orders.Contracts;
using VsaTemplate.Features.Products.Domain;
using VsaTemplate.Tests.Common;

namespace VsaTemplate.Tests.Features.Orders;

#if (IncludeComments)
// Test unitario del ESTILO B (handler de instancia): se construye con new,
// pasando las dependencias al constructor — tampoco requiere interfaz.
// TimeProvider y IEventDispatcher muestran cómo se controlan dependencias con
// más de una implementación: fakes aquí, las reales en producción.
#endif
public class PlaceOrderTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 14, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Calcula_el_total_con_el_precio_actual_del_producto()
    {
        await using var db = TestDb.Create();
        var product = new Product { Id = Guid.NewGuid(), Name = "Mouse", Price = 50m };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        var handler = new PlaceOrderHandler(db, new FixedClock(Now), new RecordingDispatcher());

        var result = await handler.Handle(
            new PlaceOrder.Request(product.Id, 3), CancellationToken.None);

        var created = Assert.IsType<Created<PlaceOrder.Response>>(result.Result);
        Assert.Equal(150m, created.Value!.Total);
        Assert.Equal(Now, created.Value.CreatedAt);
    }

    [Fact]
    public async Task Publica_el_evento_OrderPlaced_al_colocar_la_orden()
    {
        await using var db = TestDb.Create();
        var product = new Product { Id = Guid.NewGuid(), Name = "Mouse", Price = 50m };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        var dispatcher = new RecordingDispatcher();
        var handler = new PlaceOrderHandler(db, new FixedClock(Now), dispatcher);

        await handler.Handle(new PlaceOrder.Request(product.Id, 3), CancellationToken.None);

        var evento = Assert.IsType<OrderPlaced>(Assert.Single(dispatcher.Published));
        Assert.Equal(product.Id, evento.ProductId);
        Assert.Equal(3, evento.Quantity);
        Assert.Equal(150m, evento.Total);
    }

    [Fact]
    public async Task Rechaza_la_orden_cuando_el_producto_no_existe()
    {
        await using var db = TestDb.Create();
        var dispatcher = new RecordingDispatcher();
        var handler = new PlaceOrderHandler(db, new FixedClock(Now), dispatcher);

        var result = await handler.Handle(
            new PlaceOrder.Request(Guid.NewGuid(), 1), CancellationToken.None);

        Assert.IsType<NotFound<string>>(result.Result);
        Assert.Empty(db.Orders);
        Assert.Empty(dispatcher.Published);
    }

    [Fact]
    public void El_validator_rechaza_la_cantidad_no_positiva()
    {
        var result = new PlaceOrder.Validator()
            .Validate(new PlaceOrder.Request(Guid.NewGuid(), 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Quantity");
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class RecordingDispatcher : IEventDispatcher
    {
        public List<object> Published { get; } = [];

        public Task Publish<TEvent>(TEvent evento, CancellationToken ct = default)
            where TEvent : class
        {
            Published.Add(evento);
            return Task.CompletedTask;
        }
    }
}
