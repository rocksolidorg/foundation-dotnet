namespace RockSolid.Foundation.Modeling;

public class AggregateRoot<TSelf, TId> : Entity<TSelf, TId>, IAggregateRoot<TSelf, TId>
    where TSelf : AggregateRoot<TSelf, TId>
    where TId : notnull
{
    public DateTimeOffset LastModifiedAt { get; protected set; }

    protected AggregateRoot(TId id, DateTimeOffset lastModifiedAt = default) : base(id)
    {
        LastModifiedAt = lastModifiedAt;
    }
}