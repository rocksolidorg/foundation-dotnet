using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace RockSolid.Foundation.Modeling.AspNetCore;

public class UnitOfWork<TContext> : IUnitOfWork
    where TContext : DbContext
{
    private readonly TContext _context;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly TimeProvider _timeProvider;
    private int _saveDepth = 0;
    private const int MaxDispatch = 1024;
    public UnitOfWork(TContext context, IDomainEventDispatcher domainEventDispatcher, TimeProvider timeProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _domainEventDispatcher = domainEventDispatcher ?? throw new ArgumentNullException(nameof(domainEventDispatcher));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public IRepository<TAggregate> Repository<TAggregate>()
        where TAggregate : class, IAggregateRoot
        => new EFRepository<TAggregate>(_context);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (Interlocked.Increment(ref _saveDepth) > 1)
                return;
            var seen = new HashSet<IDomainEvent>(ReferenceEqualityComparer.Instance);
            int dispatchedTotal = 0;

            while (true)
            {
                var domainEvents = _context.ChangeTracker
                    .Entries<IEntity>()
                    .SelectMany(e => e.Entity.DomainEvents)
                    .ToList();
                int dispatched = 0;
                foreach (var domainEvent in domainEvents)
                {
                    if (seen.Add(domainEvent))
                    {
                        await _domainEventDispatcher.DispatchAsync(domainEvent, cancellationToken);
                        ++dispatched;
                    }
                }
                if (dispatched == 0)
                    break;
                dispatchedTotal += dispatched;
                if (dispatchedTotal > MaxDispatch)
                    throw new InvalidOperationException("Too many domain events dispatched during SaveChangesAsync. Possible infinite loop detected.");
            }

            var now = _timeProvider.GetUtcNow();

            foreach (var entry in _context.ChangeTracker.Entries<IAggregateRoot>())
            {
                if (entry.State is EntityState.Added or EntityState.Modified)
                {
                    entry.Property("LastModifiedAt").CurrentValue = now;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            foreach (var entry in _context.ChangeTracker.Entries<IEntity>())
            {
                entry.Entity.ClearDomainEvents();
            }

        }
        finally
        {
            Interlocked.Decrement(ref _saveDepth);
        }
    }

    public Task ExecuteDeleteAsync<TAggregate>(Expression<Func<TAggregate, bool>> predicate, CancellationToken cancellationToken)
        where TAggregate : class, IAggregateRoot
        => _context
                .Set<TAggregate>()
                .Where(predicate)
                .ExecuteDeleteAsync(cancellationToken);

    internal sealed class EFRepository<TAggregate>(DbContext db) : IRepository<TAggregate>
        where TAggregate : class, IAggregateRoot
    {
        private readonly DbSet<TAggregate> _set = db.Set<TAggregate>();

        public ValueTask<TAggregate?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken)
            where TId : notnull, IComparable<TId>, IEquatable<TId>
            => _set.FindAsync(id, cancellationToken);

        public IAsyncEnumerable<TAggregate> GetBySpecificationAsync(Expression<Func<TAggregate, bool>> specification, CancellationToken cancellationToken)
            => _set
                .AsNoTracking()
                .Where(specification)
                .AsAsyncEnumerable();

        public IAsyncEnumerable<TAggregate> GetBySpecificationForUpdateAsync(Expression<Func<TAggregate, bool>> specification, CancellationToken cancellationToken)
            => _set
                .AsTracking()
                .Where(specification)
                .AsAsyncEnumerable();

        public void Add(TAggregate aggregate)
            => _set.Add(aggregate);

        public void Update(TAggregate aggregate)
            => _set.Update(aggregate);

        public void Remove(TAggregate aggregate)
            => _set.Remove(aggregate);

    }
}

