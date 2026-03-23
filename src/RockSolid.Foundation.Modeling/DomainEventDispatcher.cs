using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace RockSolid.Foundation.Modeling;

public sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    private delegate Task DomainEventHandlerDelegate(object handler, IDomainEvent domainEvent, CancellationToken cancellationToken);
    private static readonly ConcurrentDictionary<Type, (Type, DomainEventHandlerDelegate)> _handlerCache = [];
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        var (enumerableType, delegateMethod) = _handlerCache.GetOrAdd(
            domainEvent.GetType(),
            static type =>
            {
                var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(type);
                return (
                    typeof(IEnumerable<>).MakeGenericType(handlerType),
                    CreateHandlerDelegate(handlerType.GetMethod(
                        nameof(IDomainEventHandler<>.HandleAsync),
                        BindingFlags.Public | BindingFlags.Instance,
                        null,
                        [type, typeof(CancellationToken)],
                        null
                    )!, type)
                );
            });

        if (_serviceProvider.GetService(enumerableType) is IEnumerable<object> handlers)
        {
            foreach (var handler in handlers)
            {

                await delegateMethod(handler, domainEvent, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }

    private static DomainEventHandlerDelegate CreateHandlerDelegate(MethodInfo methodInfo, Type eventType)
    {
        // Parameters: (object handler, object domainEvent, CancellationToken cancellationToken)
        var handlerParam = Expression.Parameter(typeof(object));
        var eventParam = Expression.Parameter(typeof(IDomainEvent));
        var tokenParam = Expression.Parameter(typeof(CancellationToken));

        // Cast handler and domainEvent to their actual types
        var castHandler = Expression.Convert(handlerParam, methodInfo.DeclaringType!);
        var castEvent = Expression.Convert(eventParam, eventType);

        // Call ((IDomainEventHandler<TEvent>)handler).HandleAsync((TEvent)domainEvent, cancellationToken)
        var call = Expression.Call(
            castHandler,
            methodInfo,
            castEvent,
            tokenParam
        );

        // Lambda: (object handler, object domainEvent, CancellationToken cancellationToken) => handler.HandleAsync((TEvent)domainEvent, cancellationToken)
        var lambda = Expression.Lambda<DomainEventHandlerDelegate>(call, handlerParam, eventParam, tokenParam);
        return lambda.Compile();
    }
}