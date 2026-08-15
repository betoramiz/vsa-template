using System.Security.Claims;

namespace VsaTemplate.Common.Auth;

#if (IncludeComments)
/// <summary>
/// Abstrae el acceso al usuario autenticado para que los casos de uso no
/// dependan de HttpContext directamente: eso los volvería intestables sin un
/// host web y filtraría un detalle de transporte dentro del dominio.
/// </summary>
#endif
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    string? UserId { get; }
    IReadOnlyCollection<string> Roles { get; }
}

internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public string? UserId => Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

    public IReadOnlyCollection<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ?? [];
}
