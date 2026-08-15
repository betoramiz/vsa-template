namespace VsaTemplate.Composition;

public static class UseCaseRegistration
{
#if (IncludeComments)
    /// <summary>
    /// Registra por convención todo handler de caso de uso (clase pública,
    /// no abstracta, cuyo nombre termina en "Handler") como scoped. Un
    /// feature nuevo no toca este archivo: agregar la clase alcanza.
    /// Vive fuera de Common: aunque el scan no genera una dependencia de
    /// tipo hacia los slices (no hay "using Features.X" ni tipos referenciados
    /// en el código, así que ArchitectureTests no lo marca), sigue siendo
    /// wiring de toda la app — la raíz de composición, junto con Program.cs.
    /// </summary>
#endif
    public static IServiceCollection AddUseCaseHandlers(this IServiceCollection services) =>
        services.Scan(scan => scan
            .FromAssemblyOf<Program>()
            .AddClasses(classes => classes.Where(t => t is { IsPublic: true, IsAbstract: false } && t.Name.EndsWith("Handler")))
            .AsSelf()
            .WithScopedLifetime());
}
