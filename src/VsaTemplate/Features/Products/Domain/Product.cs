using VsaTemplate.Common.Persistence;

namespace VsaTemplate.Features.Products.Domain;

public class Product : IHasRowVersion
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
