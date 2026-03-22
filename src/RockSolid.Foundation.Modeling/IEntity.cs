namespace RockSolid.Foundation.Modeling;

public interface IEntity
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
    bool Transient { get; }
}

public interface IEntity<TSelf, TId> : IEntity, IEquatable<IEntity<TSelf, TId>>
    where TSelf : IEntity<TSelf, TId>
    where TId : notnull
{
    public TId Id { get; }
}