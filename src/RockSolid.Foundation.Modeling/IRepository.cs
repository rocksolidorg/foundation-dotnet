using System.Linq.Expressions;

namespace RockSolid.Foundation.Modeling;

public interface IRepository<TAggregate> : IEnumerable<TAggregate>, IQueryable<TAggregate>
    where TAggregate : class, IAggregateRoot
{
    ValueTask<TAggregate?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken)
        where TId : notnull, IComparable<TId>, IEquatable<TId>;
    void Add(TAggregate aggregate);
    void Update(TAggregate aggregate);
    void Remove(TAggregate aggregate);
}