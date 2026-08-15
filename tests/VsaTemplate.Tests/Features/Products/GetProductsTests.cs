using VsaTemplate.Features.Products;
using VsaTemplate.Features.Products.Domain;
using VsaTemplate.Tests.Common;

namespace VsaTemplate.Tests.Features.Products;

public class GetProductsTests
{
    [Fact]
    public async Task Devuelve_la_pagina_pedida_y_el_total_de_todos_los_productos()
    {
        await using var db = TestDb.Create();
        for (var i = 1; i <= 5; i++)
            db.Products.Add(new Product { Id = Guid.NewGuid(), Name = $"Producto {i}", Price = i * 10m });
        await db.SaveChangesAsync();
        var handler = new GetProductsHandler(db);

        var result = await handler.Handle(new GetProducts.Request(PageNumber: 2, PageSize: 2), CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasNextPage);
        Assert.Equal(["Producto 3", "Producto 4"], result.Items.Select(p => p.Name));
    }

    [Fact]
    public void El_validator_rechaza_page_number_menor_a_uno()
    {
        var result = new GetProducts.Validator().Validate(new GetProducts.Request(PageNumber: 0, PageSize: 20));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PageNumber");
    }

    [Fact]
    public void El_validator_rechaza_page_size_fuera_de_rango()
    {
        var result = new GetProducts.Validator().Validate(new GetProducts.Request(PageNumber: 1, PageSize: 500));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PageSize");
    }
}
