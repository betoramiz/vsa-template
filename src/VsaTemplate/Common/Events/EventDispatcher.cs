namespace VsaTemplate.Common.Events;

#if (IncludeComments)
/// <summary>
/// Despacha en secuencia a todos los handlers registrados para el tipo del
/// evento, dentro del mismo scope del request (comparten DbContext, etc.).
/// Un evento sin suscriptores es un no-op válido.
/// </summary>
#endif
public class EventDispatcher(IServiceProvider services) : IEventDispatcher
{
    public async Task Publish<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class
    {
        foreach (var handler in services.GetServices<IEventHandler<TEvent>>())
            await handler.Handle(@event, ct);
    }
}

public static class EventsRegistration
{
#if (IncludeComments)
    /// <summary>
    /// Registra el dispatcher y descubre todos los IEventHandler&lt;T&gt; del
    /// assembly. Agregar un suscriptor nuevo = crear el archivo, nada más.
    /// </summary>
#endif
    public static IServiceCollection AddEvents(this IServiceCollection services)
    {
        services.AddScoped<IEventDispatcher, EventDispatcher>();

        var handlerRegistrations = typeof(EventsRegistration).Assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                .Select(i => (Service: i, Implementation: t)));

        foreach (var (service, implementation) in handlerRegistrations)
            services.AddScoped(service, implementation);

        return services;
    }
}
