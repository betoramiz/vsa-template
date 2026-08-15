using Microsoft.Extensions.DependencyInjection;
using VsaTemplate.Common.Events;
using VsaTemplate.Features.Notifications;
using VsaTemplate.Features.Products.Contracts;

namespace VsaTemplate.Tests.Common;

public class EventDispatcherTests
{
    [Fact]
    public async Task Invokes_all_handlers_subscribed_to_the_event()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Recorder>();
        services.AddScoped<IEventDispatcher, EventDispatcher>();
        services.AddScoped<IEventHandler<TestEvent>, HandlerA>();
        services.AddScoped<IEventHandler<TestEvent>, HandlerB>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var dispatcher = scope.ServiceProvider.GetRequiredService<IEventDispatcher>();
        await dispatcher.Publish(new TestEvent("hola"));

        var recorder = provider.GetRequiredService<Recorder>();
        Assert.Equal(["A:hola", "B:hola"], recorder.Calls);
    }

    [Fact]
    public async Task Publishing_an_event_without_subscribers_does_not_fail()
    {
        var services = new ServiceCollection();
        services.AddScoped<IEventDispatcher, EventDispatcher>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var dispatcher = scope.ServiceProvider.GetRequiredService<IEventDispatcher>();

        await dispatcher.Publish(new TestEvent("nadie escucha")); // no lanza
    }

    [Fact]
    public void AddEvents_discovers_the_handlers_in_the_api_assembly()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddEvents();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IEventHandler<ProductCreated>>();
        Assert.Contains(handlers, h => h is ProductCreatedNotification);
    }

    private sealed record TestEvent(string Message);

    private sealed class Recorder
    {
        public List<string> Calls { get; } = [];
    }

    private sealed class HandlerA(Recorder recorder) : IEventHandler<TestEvent>
    {
        public Task Handle(TestEvent @event, CancellationToken ct)
        {
            recorder.Calls.Add($"A:{@event.Message}");
            return Task.CompletedTask;
        }
    }

    private sealed class HandlerB(Recorder recorder) : IEventHandler<TestEvent>
    {
        public Task Handle(TestEvent @event, CancellationToken ct)
        {
            recorder.Calls.Add($"B:{@event.Message}");
            return Task.CompletedTask;
        }
    }
}
