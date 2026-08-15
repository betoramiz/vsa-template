namespace VsaTemplate.Common.Endpoints;

#if (IncludeComments)
/// <summary>
/// Contrato para el descubrimiento automático de endpoints.
/// Cada feature expone sus rutas implementando esta interfaz en su clase
/// XxxEndpoints; <see cref="EndpointDiscovery.MapFeatureEndpoints"/> las
/// encuentra por reflection al arrancar. Agregar un feature nuevo no
/// requiere tocar Program.cs.
/// </summary>
#endif
public interface IFeatureEndpoints
{
    static abstract void Map(IEndpointRouteBuilder app);
}

public static class EndpointDiscovery
{
    public static IEndpointRouteBuilder MapFeatureEndpoints(this IEndpointRouteBuilder app)
    {
        var endpointTypes = typeof(EndpointDiscovery).Assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && t.IsAssignableTo(typeof(IFeatureEndpoints)));

        foreach (var type in endpointTypes)
            type.GetMethod(nameof(IFeatureEndpoints.Map))!.Invoke(null, [app]);

        return app;
    }
}
