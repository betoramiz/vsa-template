using Moq;
using VsaTemplate.Common.Events;
using VsaTemplate.Features.Products;
using VsaTemplate.Tests.Common;

namespace VsaTemplate.Tests.Features.Products;

public class CreateProductTests
{
    [Fact]
    public async Task Creates_the_product_when_the_data_is_valid()
    {
        var moq = new Mock<IEventDispatcher>();
        await using var db = TestDb.Create();
        
        var handler = new CreateProductHandler(db, moq.Object);

        var request = new CreateProduct.Request("Teclado", 99m);
        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal("Teclado", result.Value!.Name);
        Assert.Single(db.Products);
    }

    [Fact]
    public void The_validator_rejects_the_empty_name()
    {
        var result = new CreateProduct.Validator()
            .Validate(new CreateProduct.Request("   ", 99m));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void The_validator_rejects_the_non_positive_price()
    {
        var result = new CreateProduct.Validator()
            .Validate(new CreateProduct.Request("Teclado", 0m));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Price");
    }
}
