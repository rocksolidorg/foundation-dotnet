using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace RockSolid.Foundation.Modeling.AspNetCore;

internal sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    private delegate Task DispatchDelegate(IServiceProvider serviceProvider, IDomainEvent domainEvent, CancellationToken cancellationToken);
    private static readonly ConcurrentDictionary<Type, DispatchDelegate> _cache = [];
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    public Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        var dispatch = _cache.GetOrAdd(
            domainEvent.GetType(),
            static type => typeof(DomainEventDispatcher)
                .GetMethod(nameof(DispatchCore), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(type)
                .CreateDelegate<DispatchDelegate>()
        );
        return dispatch(_serviceProvider, domainEvent, cancellationToken);
    }

    private static async Task DispatchCore<TEvent>(IServiceProvider serviceProvider, IDomainEvent domainEvent, CancellationToken cancellationToken)
        where TEvent : IDomainEvent
    {
        var handlers = serviceProvider.GetService<IEnumerable<IDomainEventHandler<TEvent>>>();
        if (handlers is not null)
        {
            foreach (var handler in handlers)
            {
                await handler.HandleAsync((TEvent)domainEvent, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
