using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RockSolid.Foundation.Modeling.AspNetCore;

public static class Extensions
{

    extension(ModelBuilder modelBuilder)
    {
        public ModelBuilder AddOutbox()
        {
            modelBuilder.ApplyConfiguration(new OutboxMessageEntityTypeConfiguration());
            return modelBuilder;
        }
    }

    extension(IServiceCollection services)
    {

        public IServiceCollection AddUnitOfWork<TContext>()
            where TContext : DbContext, IOutboxContext
        {
            services.TryAddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            services.TryAddScoped<IUnitOfWork, UnitOfWork<TContext>>();
            return services;
        }

        public IServiceCollection AddDomainEventHandler<THandler>()
            where THandler : class
        {
            var handlerType = typeof(THandler);
            var handlerInterfaces = handlerType.GetInterfaces().Where(IsDomainEventHandlerInterface).ToList();

            if (!IsValidType(handlerType, handlerInterfaces))
                throw new ArgumentException(
                    "THandler must be a closed (non-generic), non-abstract class type that implements one or more IDomainEventHandler<> interfaces.",
                    nameof(THandler)
                );

            services.TryAddScoped<THandler>();
            foreach (var handlerInterface in handlerInterfaces)
            {
                services.AddScoped(handlerInterface, sp => sp.GetRequiredService<THandler>());
            }
            return services;
        }
    }

    private static bool IsDomainEventHandlerInterface(Type type)
        => type.IsGenericType
        && type.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)
        && !type.ContainsGenericParameters;

    private static bool IsValidType(Type type, IList<Type> interfaces)
        => !type.IsAbstract
        && !type.IsGenericTypeDefinition
        && interfaces.Count > 0;

}
