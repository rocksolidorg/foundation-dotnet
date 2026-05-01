using System.Security.Cryptography;

namespace RockSolid.Foundation.Modeling;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);

}
