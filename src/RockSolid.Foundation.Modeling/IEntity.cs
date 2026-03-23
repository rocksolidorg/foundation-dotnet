namespace RockSolid.Foundation.Modeling;

public interface IEntity
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    bool Transient { get; }
    void ClearDomainEvents();
}

public interface IEntity<TSelf, TId> : IEntity, IEquatable<IEntity<TSelf, TId>>
    where TSelf : IEntity<TSelf, TId>
    where TId : notnull, IComparable<TId>, IEquatable<TId>
{
    public TId Id { get; }
}