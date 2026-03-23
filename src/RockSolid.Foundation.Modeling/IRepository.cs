using System.Linq.Expressions;

namespace RockSolid.Foundation.Modeling;

public interface IRepository<TAggregate>
    where TAggregate : class, IAggregateRoot
{
    ValueTask<TAggregate?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken)
        where TId : notnull;
    IAsyncEnumerable<TAggregate> GetBySpecificationAsync(Expression<Func<TAggregate, bool>> specification, CancellationToken cancellationToken);
    IAsyncEnumerable<TAggregate> GetBySpecificationForUpdateAsync(Expression<Func<TAggregate, bool>> specification, CancellationToken cancellationToken);
    void Add(TAggregate aggregate);
    void Update(TAggregate aggregate);
    void Remove(TAggregate aggregate);
}