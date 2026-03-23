namespace RockSolid.Foundation.Modeling;

public interface IDomainEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    Task HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken);
}
