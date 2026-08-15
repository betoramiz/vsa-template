using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using VsaTemplate.Common.Pagination;
using VsaTemplate.Features.Products;
using VsaTemplate.Tests.Common;

namespace VsaTemplate.Tests.Integration;

#if (IncludeComments)
// Test de integración: pega a la API por HTTP real (routing, serialización,
// binding y DI incluidos). Cubre lo que un unitario del handler no puede:
// que la ruta existe, que el JSON mapea al Request, que el status code sale.
#endif
public class ProductsApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task The_create_get_delete_cycle_works_end_to_end()
    {
        var create = await _client.PostAsJsonAsync("/products", new { name = "Monitor", price = 300m });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<CreateProduct.Response>();

        var get = await _client.GetAsync($"/products/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var fetched = await get.Content.ReadFromJsonAsync<GetProductById.Response>();
        Assert.Equal("Monitor", fetched!.Name);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.CreateToken(roles: ["Admin"]));
        var delete = await _client.DeleteAsync($"/products/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var getAfterDelete = await _client.GetAsync($"/products/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
    }

    [Fact]
    public async Task Deleting_a_product_without_a_bearer_token_returns_401()
    {
        var create = await _client.PostAsJsonAsync("/products", new { name = "Keyboard", price = 50m });
        var created = await create.Content.ReadFromJsonAsync<CreateProduct.Response>();

        var delete = await _client.DeleteAsync($"/products/{created!.Id}");

        Assert.Equal(HttpStatusCode.Unauthorized, delete.StatusCode);
    }

    [Fact]
    public async Task Deleting_a_product_with_a_token_that_lacks_the_admin_role_returns_403()
    {
        var create = await _client.PostAsJsonAsync("/products", new { name = "Webcam", price = 40m });
        var created = await create.Content.ReadFromJsonAsync<CreateProduct.Response>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.CreateToken());
        var delete = await _client.DeleteAsync($"/products/{created!.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
    }

    [Fact]
    public async Task Returns_400_with_validation_detail_when_the_request_is_invalid()
    {
        var response = await _client.PostAsJsonAsync("/products", new { name = "", price = -1m });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Lista_los_productos_de_forma_paginada()
    {
        await _client.PostAsJsonAsync("/products", new { name = "Teclado", price = 50m });
        await _client.PostAsJsonAsync("/products", new { name = "Mouse", price = 20m });

        var response = await _client.GetAsync("/products?pageNumber=1&pageSize=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResponse<GetProducts.Response>>();
        Assert.Single(page!.Items);
        Assert.True(page.TotalCount >= 2);
    }

    [Fact]
    public async Task Devuelve_400_cuando_el_page_size_pedido_es_invalido()
    {
        var response = await _client.GetAsync("/products?pageNumber=1&pageSize=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
