namespace RockSolid.Foundation.Modeling;

public interface IValueObject { }

public interface IValueObject<TSelf> : IValueObject, IEquatable<TSelf>
    where TSelf : IValueObject<TSelf>
{ }
