using Microsoft.Extensions.DependencyInjection;
using MinimalClean.Application.Abstractions;
using MinimalClean.Domain.Common;

namespace MinimalClean.Infrastructure.Events;

public class DomainEventDispatcher
{
    private readonly IServiceProvider _sp;
    public DomainEventDispatcher(IServiceProvider sp) => _sp = sp;

    public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct)
    {
        using var scope = _sp.CreateScope();
        foreach (var ev in events)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(ev.GetType());
            var handlers = scope.ServiceProvider.GetServices(handlerType);
            foreach (var handler in handlers)
            {
                var method = handlerType.GetMethod("Handle")!;
                var task = (Task)method.Invoke(handler, new object[] { ev, ct })!;
                await task;
            }
        }
    }
}
