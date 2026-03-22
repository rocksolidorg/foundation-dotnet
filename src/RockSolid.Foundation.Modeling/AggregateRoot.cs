namespace RockSolid.Foundation.Modeling;

public class AggregateRoot<TSelf, TId> : Entity<TSelf, TId>, IAggregateRoot<TSelf, TId>
    where TSelf : AggregateRoot<TSelf, TId>
    where TId : notnull
{
    public DateTimeOffset LastModifiedAt { get; protected set; } = DateTimeOffset.MinValue;

    protected AggregateRoot(TId id) : base(id) { }
}