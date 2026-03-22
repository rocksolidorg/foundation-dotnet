namespace RockSolid.Foundation.Modeling;

public abstract class Entity<TSelf, TId>(TId id) : IEntity<TSelf, TId>
    where TId : notnull
{

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    public TId Id { get; protected set; } = id;

    public bool Transient => false;

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
        => _domainEvents.Clear();

    public override int GetHashCode()
        => HashCode.Combine(GetType(), Id);

    public override bool Equals(object? other)
        => Equals(other as Entity<TSelf, TId>);

    public bool Equals(IEntity<TSelf, TId>? other)
        => (other is not null) && GetType() == other.GetType() && EqualityComparer<TId>.Default.Equals(Id, other.Id);

    public static bool operator ==(Entity<TSelf, TId>? left, Entity<TSelf, TId>? right)
        => Equals(left, right);

    public static bool operator !=(Entity<TSelf, TId>? left, Entity<TSelf, TId>? right)
        => !Equals(left, right);

}