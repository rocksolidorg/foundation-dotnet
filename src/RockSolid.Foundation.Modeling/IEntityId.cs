namespace RockSolid.Foundation.Modeling;

public interface IEntityId<TSelf, TValue> : IValueObject<TSelf>, IComparable<TSelf>
    where TSelf : IEntityId<TSelf, TValue>
    where TValue : struct, IComparable<TValue>, IEquatable<TValue>
{
    public TValue Value { get; }
    int IComparable<TSelf>.CompareTo(TSelf? other)
        => Value.CompareTo(other?.Value ?? default);
}

