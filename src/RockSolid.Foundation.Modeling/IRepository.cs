using System.Linq.Expressions;

namespace RockSolid.Foundation.Modeling;

public interface IRepository<TAggregate, TId>
    where TAggregate : IAggregateRoot
    where TId : notnull
{
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken);
    Task<TAggregate?> GetByIdForUpdateAsync(TId id, CancellationToken cancellationToken);
    IAsyncEnumerable<TAggregate> GetBySpecificationAsync(Expression<Func<TAggregate, bool>> specification, CancellationToken cancellationToken);
    void Add(TAggregate aggregate);
    void Update(TAggregate aggregate);
    void Delete(TAggregate aggregate);
    void Delete(TId id);
    Task DeleteAsync(Expression<Func<TAggregate, bool>> predicate, CancellationToken cancellationToken);
}