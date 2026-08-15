using VsaTemplate.Common.Events;
using VsaTemplate.Features.Orders.Contracts;

namespace VsaTemplate.Features.Notifications;

#if (IncludeComments)
// Slice consumidor: reacciona a eventos de otros slices referenciando
// únicamente su contrato público (el record OrderPlaced). Orders no sabe que
// este handler existe — ese es el desacoplamiento que da el dispatcher.
// No requiere registro en Program.cs: AddEvents() lo descubre por implementar
// IEventHandler<>.
#endif
public class OrderPlacedNotification(ILogger<OrderPlacedNotification> logger)
    : IEventHandler<OrderPlaced>
{
    public Task Handle(OrderPlaced evento, CancellationToken ct)
    {
#if (IncludeComments)
        // Demo: en un caso real aquí iría enviar email/push a través de una
        // interfaz de frontera (IEmailSender).
#endif
        logger.LogInformation(
            "Orden {OrderId} colocada: {Quantity} x producto {ProductId}, total {Total}",
            evento.OrderId, evento.Quantity, evento.ProductId, evento.Total);

        return Task.CompletedTask;
    }
}
