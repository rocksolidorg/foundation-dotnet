// using System.Collections.Concurrent;

// namespace RockSolid.Foundation.Modeling;

// internal sealed class DomainEventDispatcher : IDomainEventDispatcher
// {
//     private delegate Task DispatchDelegate(IServiceProvider serviceProvider, IDomainEvent domainEvent, CancellationToken cancellationToken);
//     private static readonly ConcurrentDictionary<Type, DispatchDelegate> _cache = [];
//     private readonly IServiceProvider _serviceProvider;

//     public DomainEventDispatcher(IServiceProvider serviceProvider)
//     {
//         ArgumentNullException.ThrowIfNull(serviceProvider);
//         _serviceProvider = serviceProvider;
//     }

//     public Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
//     {
//         ArgumentNullException.ThrowIfNull(domainEvent);
//         var dispatch = _cache.GetOrAdd(
//             domainEvent.GetType(),
//             static type => typeof(DomainEventDispatcher)
//                 .GetMethod(nameof(DispatchCore), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
//                 .MakeGenericMethod(type)
//                 .CreateDelegate<DispatchDelegate>()
//         );
//         return dispatch(_serviceProvider, domainEvent, cancellationToken);
//     }

//     private static async Task DispatchCore<TEvent>(IServiceProvider serviceProvider, IDomainEvent domainEvent, CancellationToken cancellationToken)
//         where TEvent : IDomainEvent
//     {
//         if (serviceProvider.GetService(typeof(IEnumerable<IDomainEventHandler<TEvent>>))
//             is IEnumerable<IDomainEventHandler<TEvent>> handlers)
//         {
//             foreach (var handler in handlers)
//             {
//                 await handler.HandleAsync((TEvent)domainEvent, cancellationToken)
//                     .ConfigureAwait(false);
//             }
//         }
//     }
// }

