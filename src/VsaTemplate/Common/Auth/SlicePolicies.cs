using Microsoft.AspNetCore.Authorization;

namespace VsaTemplate.Common.Auth;

#if (IncludeComments)
/// <summary>
/// Contrato para que cada slice declare sus propias policies de autorización.
/// Common/Auth no conoce roles ni reglas de negocio de ningún slice: solo
/// descubre estas clases por reflection e invoca Configure() al arrancar
/// (mismo patrón que IFeatureEndpoints/EndpointDiscovery), así que nunca hay
/// un "using Features.X" en Common y ArchitectureTests no lo marca como
/// violación aunque el slice sí queda "conectado" a la app.
/// </summary>
#endif
public interface ISlicePolicies
{
    static abstract void Configure(AuthorizationOptions options);
}

internal static class SlicePolicyDiscovery
{
    public static void ConfigureSlicePolicies(this AuthorizationOptions options)
    {
        var policyTypes = typeof(ISlicePolicies).Assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && t.IsAssignableTo(typeof(ISlicePolicies)));

        foreach (var type in policyTypes)
            type.GetMethod(nameof(ISlicePolicies.Configure))!.Invoke(null, [options]);
    }
}
