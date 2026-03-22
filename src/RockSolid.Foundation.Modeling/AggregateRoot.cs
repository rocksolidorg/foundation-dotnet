namespace RockSolid.Foundation.Modeling;

public class AggregateRoot<TSelf, TId> : Entity<TSelf, TId>, IAggregateRoot
    where TId : notnull
{
    public DateTimeOffset LastModifiedAt { get; protected set; } = DateTimeOffset.MinValue;

    protected AggregateRoot(TId id) : base(id) { }
}