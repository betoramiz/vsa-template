using System.Net;
using System.Net.Http.Json;
using VsaTemplate.Features.Orders;
using VsaTemplate.Features.Products;
using VsaTemplate.Tests.Common;

namespace VsaTemplate.Tests.Integration;

#if (IncludeComments)
// Mismo tipo de test de integración, pero contra el feature del ESTILO B:
// verifica que el handler de instancia se resuelve del contenedor de DI.
#endif
public class OrdersApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Coloca_una_orden_sobre_un_producto_existente()
    {
        var createProduct = await _client.PostAsJsonAsync("/products", new { name = "Laptop", price = 1000m });
        var product = await createProduct.Content.ReadFromJsonAsync<CreateProduct.Response>();

        var placeOrder = await _client.PostAsJsonAsync("/orders", new { productId = product!.Id, quantity = 2 });

        Assert.Equal(HttpStatusCode.Created, placeOrder.StatusCode);
        var order = await placeOrder.Content.ReadFromJsonAsync<PlaceOrder.Response>();
        Assert.Equal(2000m, order!.Total);

        var get = await _client.GetAsync($"/orders/{order.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
    }

    [Fact]
    public async Task Devuelve_404_cuando_el_producto_de_la_orden_no_existe()
    {
        var response = await _client.PostAsJsonAsync("/orders", new { productId = Guid.NewGuid(), quantity = 1 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
