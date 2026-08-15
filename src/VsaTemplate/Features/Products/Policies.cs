using Microsoft.AspNetCore.Authorization;
using VsaTemplate.Common.Auth;

namespace VsaTemplate.Features.Products;

#if (IncludeComments)
/// <summary>
/// Policies de autorización propias de este slice. Common/Auth las descubre
/// por reflection (ISlicePolicies) e invoca Configure() al arrancar — el
/// slice no toca Program.cs ni Common para declarar sus reglas. No genera una
/// dependencia de tipo desde Common (nada la referencia por nombre, solo
/// reflection), así que ArchitectureTests no la marca como violación aunque
/// el slice sí queda conectado a la app.
/// </summary>
#endif
public sealed class Policies : ISlicePolicies
{
    public const string DeleteProducts = "Products.Delete";

    public static void Configure(AuthorizationOptions options) =>
        options.AddPolicy(DeleteProducts, policy => policy.RequireRole("Admin"));
}
