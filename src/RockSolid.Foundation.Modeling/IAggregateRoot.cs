namespace RockSolid.Foundation.Modeling;

public interface IAggregateRoot : IEntity
{
    DateTimeOffset LastModifiedAt { get; }
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();

}

public interface IAggregateRoot<TSelf, TId> : IAggregateRoot, IEntity<TSelf, TId>
    where TSelf : IAggregateRoot<TSelf, TId>
    where TId : notnull, IComparable<TId>, IEquatable<TId>
{
}