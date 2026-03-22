namespace RockSolid.Foundation.Modeling;

public interface IDomainEvent
{

}

public interface IDomainEvent<TSelf> : IDomainEvent, IEquatable<TSelf>
{

}