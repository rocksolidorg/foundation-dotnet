namespace RockSolid.Foundation.Modeling;

public interface IEntity
{
    bool Transient { get; }
}

public interface IEntity<TSelf, TId> : IEntity, IEquatable<IEntity<TSelf, TId>>
    where TSelf : IEntity<TSelf, TId>
    where TId : notnull, IComparable<TId>, IEquatable<TId>
{
    public TId Id { get; }
}