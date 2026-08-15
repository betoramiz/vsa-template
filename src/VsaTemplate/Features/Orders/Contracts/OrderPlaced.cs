namespace VsaTemplate.Features.Orders.Contracts;

#if (IncludeComments)
// Contrato público del slice. Junto con Domain/, es lo ÚNICO que otros slices
// pueden referenciar de Orders — los casos de uso (la raíz del slice) son
// privados. La regla la verifican los tests de arquitectura
// (tests/Architecture/ArchitectureTests.cs). Los eventos publicados viven en
// Contracts/ precisamente para que esa frontera sea comprobable.
#endif
public record OrderPlaced(Guid OrderId, Guid ProductId, int Quantity, decimal Total);
