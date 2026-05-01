namespace RockSolid.Foundation.Modeling;

public abstract class AggregateRoot<TSelf, TId> : Entity<TSelf, TId>, IAggregateRoot<TSelf, TId>
    where TSelf : AggregateRoot<TSelf, TId>
    where TId : notnull, IComparable<TId>, IEquatable<TId>
{
    public DateTimeOffset LastModifiedAt { get; protected set; }

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    protected AggregateRoot(TId id, DateTimeOffset lastModifiedAt = default) : base(id)
    {
        LastModifiedAt = lastModifiedAt;
    }

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
        => _domainEvents.Clear();

}