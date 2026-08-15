using VsaTemplate.Common.Events;
using VsaTemplate.Features.Products.Contracts;

namespace VsaTemplate.Features.Notifications;

public class ProductCreatedNotification(ILogger<ProductCreated> logger): IEventHandler<ProductCreated>
{
    public Task Handle(ProductCreated @event, CancellationToken ct)
    {
        logger.LogInformation("Product Created {Name} - ${Price}",  @event.Name, @event.Price);
        return Task.CompletedTask;
    }
}