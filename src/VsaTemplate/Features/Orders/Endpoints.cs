using VsaTemplate.Common.Endpoints;

namespace VsaTemplate.Features.Orders;

public sealed class Endpoints : IFeatureEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/orders").WithTags("Orders");

        group.MapPost("/", (PlaceOrder.Request request, PlaceOrderHandler handler, CancellationToken ct)
            => handler.Handle(request, ct))
            .WithName(nameof(PlaceOrder))
            .WithValidation<PlaceOrder.Request>();

        group.MapGet("/{id:guid}", (Guid id, GetOrderByIdHandler handler, CancellationToken ct)
            => handler.Handle(id, ct)).WithName(nameof(GetOrderById));
    }
}
