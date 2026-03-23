using System.Linq.Expressions;

namespace RockSolid.Foundation.Modeling;

public interface IUnitOfWork
{
    IRepository<TAggregate> Repository<TAggregate>()
        where TAggregate : class, IAggregateRoot;
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task ExecuteDeleteAsync<TAggregate>(Expression<Func<TAggregate, bool>> predicate, CancellationToken cancellationToken)
        where TAggregate : class, IAggregateRoot;
}

