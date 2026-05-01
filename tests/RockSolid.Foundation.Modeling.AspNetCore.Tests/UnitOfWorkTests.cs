using System.Collections;
using System.Data;
using System.Linq.Expressions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Xunit.Internal;

namespace RockSolid.Foundation.Modeling.AspNetCore.Tests;

public class ExtensionTests
{

    internal sealed class ErrorType1;
    internal struct ErrorType2;
    [Fact]
    public void AddDomainEventHandler_1()
    {
        Assert.Throws<ArgumentException>(() => new ServiceCollection().AddDomainEventHandler<ErrorType1>());
    }

}


public class UnitOfWorkTests : IAsyncDisposable
{
    private readonly FakeTimeProvider _timeProvider = new()
    {
        AutoAdvanceAmount = TimeSpan.FromMicroseconds(1)
    };
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    private IServiceCollection TestServices =>
        new ServiceCollection()
            .AddSingleton<TimeProvider>(_timeProvider)
            .AddDbContext<TestDbContext>(options =>
            {
                _connection.Open();
                options.UseSqlite(_connection);
            })
            .AddUnitOfWork<TestDbContext>()
            .AddDomainEventDispatcher()
            .AddDomainEventHandler<TestHandler>();

    private (List<TestAggregate> Expected, List<TestAggregate> Entities) TestData
    {
        get
        {
            var expected = Enumerable.Range(1, 100).Select(value => new TestAggregate(
                new TestId(Guid.NewGuid()), value)).ToList();
            var extra = Enumerable.Range(101, 100).Select(value => new TestAggregate(
                new TestId(Guid.NewGuid()), value)).ToList();
            return (expected, expected.Concat(extra).OrderBy(x => x.Id).ToList());
        }
    }

    private async Task<AsyncServiceScope> CreateScopeAsync(IServiceCollection? services = null, Func<IServiceCollection, IServiceCollection>? configure = null)
    {
        configure ??= s => s;
        var provider = configure(services ?? TestServices).BuildServiceProvider();
        var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await context.Database.EnsureCreatedAsync(_cancellationToken);
        return scope;
    }

    [Fact]
    public async Task Construct_PassingNull_ThrowsArgumentNullException()
    {
        var serviceProvider = TestServices.BuildServiceProvider();
        Assert.Throws<ArgumentNullException>(() =>
            new UnitOfWork<TestDbContext>(
                null!,
                serviceProvider.GetRequiredService<IDomainEventDispatcher>(),
                serviceProvider.GetRequiredService<TimeProvider>()));
        Assert.Throws<ArgumentNullException>(() =>
            new UnitOfWork<TestDbContext>(
                serviceProvider.GetRequiredService<TestDbContext>(),
                null!,
                serviceProvider.GetRequiredService<TimeProvider>()));
        Assert.Throws<ArgumentNullException>(() =>
            new UnitOfWork<TestDbContext>(
                serviceProvider.GetRequiredService<TestDbContext>(),
                serviceProvider.GetRequiredService<IDomainEventDispatcher>(),
                null!));
    }

    [Fact]
    public async Task SaveChanges_Add_InsertsDatabase()
    {
        var expected = new TestAggregate(new(Guid.NewGuid()));

        await using (var scope = await CreateScopeAsync())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<TestAggregate>();

            repository.Add(expected);
            await unitOfWork.SaveChangesAsync(_cancellationToken);
        }

        await using (var scope = await CreateScopeAsync())
        {
            var database = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var actual = await database.Set<TestAggregate>().ToListAsync(_cancellationToken);
            Assert.Equal([expected], actual);
        }
    }

    [Fact]
    public async Task SaveChanges_MultipleAdd_InsertsIntoDatabase()
    {
        var expected = Enumerable.Range(1, 100).Select(
            i => new TestAggregate(new(Guid.NewGuid()))
        ).ToList();

        await using (var scope = await CreateScopeAsync())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<TestAggregate>();

            foreach (var item in expected)
                repository.Add(item);
            await unitOfWork.SaveChangesAsync(_cancellationToken);
        }

        await using (var scope = await CreateScopeAsync())
        {
            var database = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var actual = await database.Set<TestAggregate>().ToListAsync(_cancellationToken);
            Assert.Equal(
                expected.OrderBy(x => x.Id),
                actual.OrderBy(x => x.Id)
            );
        }
    }

    [Fact]
    public async Task SaveChanges_Updated_UpdatesDatabase()
    {
        var expected = new TestAggregate(new(Guid.NewGuid()), 99);

        await using (var scope = await CreateScopeAsync())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<TestAggregate>();

            repository.Add(new TestAggregate(expected.Id, 33));
            await unitOfWork.SaveChangesAsync(_cancellationToken);
        }

        await using (var scope = await CreateScopeAsync())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<TestAggregate>();
            var item = await repository.GetByIdAsync(expected.Id, _cancellationToken)
                ?? throw new Exception("Could not find entity");

            item.SetValue(99);
            repository.Update(item);
            await unitOfWork.SaveChangesAsync(_cancellationToken);
        }

        await using (var scope = await CreateScopeAsync())
        {
            var database = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var actual = await database.Set<TestAggregate>().ToListAsync(_cancellationToken);
            Assert.Equal([expected], actual);
        }
    }

    [Fact]
    public async Task SaveChanges_Removed_DeletesFromToDatabase()
    {
        var id = new TestId(Guid.NewGuid());
        await using (var scope = await CreateScopeAsync())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<TestAggregate>();

            repository.Add(new TestAggregate(id));
            await unitOfWork.SaveChangesAsync(_cancellationToken);
        }

        await using (var scope = await CreateScopeAsync())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<TestAggregate>();
            var item = await repository.GetByIdAsync(id, _cancellationToken)
                ?? throw new Exception("Could not find entity");

            repository.Remove(item);
            await unitOfWork.SaveChangesAsync(_cancellationToken);
        }

        await using (var scope = await CreateScopeAsync())
        {
            var database = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var actual = await database.Set<TestAggregate>().ToListAsync(_cancellationToken);
            Assert.Empty(actual);
        }
    }

    [Fact]
    public async Task SaveChanges_AddWithEvents_InvokesHandlerse()
    {
        var id = new TestId(Guid.NewGuid());
        var expected = new List<IDomainEvent>{
            new TestPropertyAdded(id, new("test", "foo")),
            new TestPropertyUpdated(id, new("test", "bar"), "foo"),
            new TestPropertyRemoved(id, new("test", "bar")),
        };
        await using var scope = await CreateScopeAsync();

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var repository = unitOfWork.Repository<TestAggregate>();
        var entity = new TestAggregate(id);
        entity.SetProperty("test", "foo");
        entity.SetProperty("test", "bar");
        entity.SetProperty("test", null);
        repository.Add(entity);
        await unitOfWork.SaveChangesAsync(_cancellationToken);

        var actual = scope.ServiceProvider.GetRequiredService<TestHandler>().ReceivedEvents;
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task SaveChanges_UpdateWithEvents_InvokesHandlerse()
    {
        var id = new TestId(Guid.NewGuid());
        var expected = new List<IDomainEvent>{
            new TestPropertyAdded(id, new("test", "foo")),
            new TestPropertyUpdated(id, new("test", "bar"), "foo"),
            new TestPropertyRemoved(id, new("test", "bar")),
        };

        await using (var scope = await CreateScopeAsync())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<TestAggregate>();
            var entity = new TestAggregate(id);
            repository.Add(entity);
            await unitOfWork.SaveChangesAsync(_cancellationToken);
        }
        await using (var scope = await CreateScopeAsync())
        {

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<TestAggregate>();
            var entity = await repository.GetByIdAsync(id, _cancellationToken)
                ?? throw new Exception("Could not find entity");
            entity.SetProperty("test", "foo");
            entity.SetProperty("test", "bar");
            entity.SetProperty("test", null);
            repository.Update(entity);
            await unitOfWork.SaveChangesAsync(_cancellationToken);

            var actual = scope.ServiceProvider.GetRequiredService<TestHandler>().ReceivedEvents;
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public async Task SaveChanges_ConcurrentUpdates_Throws()
    {
        var id = new TestId(Guid.NewGuid());
        await using (var scope = await CreateScopeAsync())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<TestAggregate>();
            var entity = new TestAggregate(id);
            repository.Add(entity);
            await unitOfWork.SaveChangesAsync(_cancellationToken);
        }

        await using var scope1 = await CreateScopeAsync();
        await using var scope2 = await CreateScopeAsync();
        var unitOfWork1 = scope1.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var unitOfWork2 = scope2.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var repository1 = unitOfWork1.Repository<TestAggregate>();
        var repository2 = unitOfWork2.Repository<TestAggregate>();
        var entity1 = await repository1.GetByIdAsync(id, _cancellationToken)
            ?? throw new Exception("Could not find entity");
        var entity2 = await repository2.GetByIdAsync(id, _cancellationToken)
            ?? throw new Exception("Could not find entity");
        entity1.SetValue(1);
        entity2.SetValue(1);

        repository1.Update(entity1);
        repository2.Update(entity2);

        await unitOfWork1.SaveChangesAsync(_cancellationToken);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            await unitOfWork2.SaveChangesAsync(_cancellationToken));

    }

    [Fact]
    public async Task Repository_ForEach_Iterates()
    {
        var expected = Enumerable.Range(1, 100).Select(i => new TestAggregate(new(Guid.NewGuid()), i)).ToList();
        await using (var scope = await CreateScopeAsync())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<TestAggregate>();
            foreach (var entity in expected)
                repository.Add(entity);
            await unitOfWork.SaveChangesAsync(_cancellationToken);
        }

        await using (var scope = await CreateScopeAsync())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<TestAggregate>();

            var actual = repository.AsEnumerable().OrderBy(x => x.Value);

            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public async Task Repository_Where_Filters()
    {
        var entities = Enumerable.Range(1, 100).Select(i => new TestAggregate(new(Guid.NewGuid()), i)).ToList();
        var expected = entities
            .Where(x => x.Value % 2 == 0)
            .OrderBy(x => x.Value).ToList();
        await using (var scope = await CreateScopeAsync())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<TestAggregate>();
            foreach (var entity in entities)
                repository.Add(entity);
            await unitOfWork.SaveChangesAsync(_cancellationToken);
        }

        await using (var scope = await CreateScopeAsync())
        {

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = unitOfWork.Repository<TestAggregate>();

            var actual = await repository
                .Where(x => x.Value % 2 == 0)
                .OrderBy(x => x.Value)
                .ToListAsync(_cancellationToken);

            Assert.Equal(expected, actual);

        }
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }

    internal sealed record TestId(Guid Value) : IEntityId<TestId, Guid>;
    internal sealed record TestProperty(string Name, string Value) : IValueObject<TestProperty>;
    internal sealed record TestPropertyAdded(TestId Id, TestProperty Property) : IDomainEvent<TestPropertyAdded>;
    internal sealed record TestPropertyUpdated(TestId Id, TestProperty Property, string OldValue) : IDomainEvent<TestPropertyUpdated>;
    internal sealed record TestPropertyRemoved(TestId Id, TestProperty Property) : IDomainEvent<TestPropertyRemoved>;
    internal sealed class TestAggregate : AggregateRoot<TestAggregate, TestId>
    {
        public int Value { get; private set; }
        private readonly List<TestProperty> _properties;
        public IReadOnlyList<TestProperty> Properties => _properties;

        public TestAggregate(TestId id, int value = 0) : base(id)
        {
            Value = value;
            _properties = [];
        }

        public TestAggregate(TestId id, int value, List<TestProperty> properties) : base(id)
        {
            Value = value;
            _properties = properties;
        }

        public void RaiseSomeEvents(IEnumerable<IDomainEvent> domainEvents)
        {
            foreach (var domainEvent in domainEvents)
                RaiseDomainEvent(domainEvent);
        }

        public void SetValue(int value)
        {
            Value = value;
        }

        public void SetProperty(string name, string? value)
        {
            int index = _properties.FindIndex(item => item.Name == name);
            if (index < 0)
            {
                if (value is not null)
                {
                    var property = new TestProperty(name, value);
                    _properties.Add(property);
                    RaiseDomainEvent(new TestPropertyAdded(Id, property));
                }
            }
            else
            {
                if (value is not null)
                {
                    var old = _properties[index];
                    if (old.Value != value)
                    {
                        var property = old with { Value = value };
                        _properties[index] = property;
                        RaiseDomainEvent(new TestPropertyUpdated(Id, property, old.Value));
                    }
                }
                else
                {
                    var property = _properties[index];
                    _properties.RemoveAt(index);
                    RaiseDomainEvent(new TestPropertyRemoved(Id, property));
                }
            }
        }
    }
    internal sealed class TestHandler :
        IDomainEventHandler<TestPropertyAdded>,
        IDomainEventHandler<TestPropertyUpdated>,
        IDomainEventHandler<TestPropertyRemoved>
    {
        public List<IDomainEvent> ReceivedEvents { get; } = [];

        public Task HandleAsync(TestPropertyAdded domainEvent, CancellationToken cancellationToken)
        {
            ReceivedEvents.Add(domainEvent);
            return Task.CompletedTask;
        }

        public Task HandleAsync(TestPropertyUpdated domainEvent, CancellationToken cancellationToken)
        {
            ReceivedEvents.Add(domainEvent);
            return Task.CompletedTask;
        }

        public Task HandleAsync(TestPropertyRemoved domainEvent, CancellationToken cancellationToken)
        {
            ReceivedEvents.Add(domainEvent);
            return Task.CompletedTask;
        }

    }
    internal sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TestAggregate> TestAggregates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TestAggregate>(entityBuilder =>
            {
                entityBuilder.HasKey(e => e.Id);
                entityBuilder.Property(e => e.Id)
                    .HasConversion(
                        p => p.Value,
                        v => new(v)
                    )
                    .IsRequired();

                entityBuilder.Property(e => e.Value);

                entityBuilder.Property<DateTimeOffset>("LastModifiedAt")
                    .IsConcurrencyToken();

                entityBuilder.OwnsMany(e => e.Properties, propertiesBuilder =>
                {
                    propertiesBuilder.WithOwner().HasForeignKey("TestId");
                    propertiesBuilder.HasKey("TestId", "Name");
                    propertiesBuilder.Property(p => p.Name);
                    propertiesBuilder.Property(p => p.Value);
                });
            });
        }

    }
}
