namespace VsaTemplate.Common.Events;

#if (IncludeComments)
/// <summary>
/// Pub/sub in-process, SIN durabilidad: si el proceso muere o un handler
/// falla, el evento se pierde (una excepción en un handler se propaga al
/// publicador y los handlers restantes no se ejecutan). Suficiente para
/// desacoplar slices dentro del mismo proceso.
/// El día que necesites garantías — outbox, retries, colas externas —
/// reemplaza esta pieza por Wolverine: los handlers ya tienen la forma
/// correcta y la migración es mecánica.
/// </summary>
#endif
public interface IEventDispatcher
{
    Task Publish<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : class;
}

#if (IncludeComments)
/// <summary>
/// A diferencia de los handlers de casos de uso, los handlers de eventos SÍ
/// usan interfaz, porque aquí el polimorfismo es real: un evento puede tener
/// N suscriptores y el dispatcher los descubre por este contrato.
/// Implementarla es todo lo necesario: AddEvents() los registra por escaneo,
/// sin tocar Program.cs.
/// </summary>
#endif
public interface IEventHandler<in TEvent> where TEvent : class
{
    Task Handle(TEvent @event, CancellationToken ct);
}
