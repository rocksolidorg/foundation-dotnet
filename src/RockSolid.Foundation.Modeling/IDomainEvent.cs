using System.Text;
using System.Text.Json;

namespace RockSolid.Foundation.Modeling;

public interface IDomainEvent;

public interface IDomainEvent<TSelf> : IDomainEvent, IEquatable<TSelf>
    where TSelf : IDomainEvent<TSelf>;

