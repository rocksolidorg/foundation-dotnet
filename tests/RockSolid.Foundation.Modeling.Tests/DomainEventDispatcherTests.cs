namespace RockSolid.Foundation.Modeling.Tests;


using Microsoft.Extensions.DependencyInjection;
using RockSolid.Foundation.Modeling;

public class DomainEventDispatcherTests
{
    private readonly CancellationToken cancellationToken = TestContext.Current.CancellationToken;
    internal sealed class TestDomainEvent : IDomainEvent { }
    internal sealed class TestDomainEventHandler : IDomainEventHandler<TestDomainEvent>
    {
        public int CalledTimes { get; private set; } = 0;

        public Task HandleAsync(TestDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            CalledTimes += 1;
            return Task.CompletedTask;
        }
    }

    internal sealed class Error : Exception { };
    internal sealed class ErrorHandler : IDomainEventHandler<TestDomainEvent>
    {
        public Task HandleAsync(TestDomainEvent domainEvent, CancellationToken cancellationToken)
            => throw new Error();
    }

    internal sealed class Test1DomainEvent : IDomainEvent { }
    internal sealed class Test2DomainEvent : IDomainEvent { }
    internal sealed class MultiHandler : IDomainEventHandler<Test1DomainEvent>, IDomainEventHandler<Test2DomainEvent>
    {
        public int CalledTimes { get; private set; } = 0;

        public Task HandleAsync(Test1DomainEvent domainEvent, CancellationToken cancellationToken)
        {
            CalledTimes += 1;
            return Task.CompletedTask;
        }

        public Task HandleAsync(Test2DomainEvent domainEvent, CancellationToken cancellationToken)
        {
            CalledTimes += 1;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task DispatchAsync_MissingServiceProvider_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new DomainEventDispatcher(null!));
    }


    [Fact]
    public async Task DispatchAsync_MissingHandler_DoesNothing()
    {
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var domainEventDispatcher = new DomainEventDispatcher(serviceProvider);

        // Throws on failure
        await domainEventDispatcher.DispatchAsync(new TestDomainEvent(), cancellationToken);

    }

    [Fact]
    public async Task DispatchAsync_SingleHandler_CallsHandler()
    {
        var handler = new TestDomainEventHandler();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IDomainEventHandler<TestDomainEvent>>(_ => handler);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var domainEventDispatcher = new DomainEventDispatcher(serviceProvider);

        await domainEventDispatcher.DispatchAsync(new TestDomainEvent(), cancellationToken);

        Assert.Equal(1, handler.CalledTimes);

    }

    [Fact]
    public async Task DispatchAsync_SingleHandler_PropagatesException()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IDomainEventHandler<TestDomainEvent>, ErrorHandler>();
        var domainEventDispatcher = new DomainEventDispatcher(serviceCollection.BuildServiceProvider());

        await Assert.ThrowsAsync<Error>(async () =>
            await domainEventDispatcher.DispatchAsync(new TestDomainEvent(), cancellationToken));

    }

    [Fact]
    public async Task DispatchAsync_MultiHandler_CallsHandler()
    {
        var handler = new MultiHandler();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IDomainEventHandler<Test1DomainEvent>>(_ => handler);
        serviceCollection.AddTransient<IDomainEventHandler<Test2DomainEvent>>(_ => handler);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var domainEventDispatcher = new DomainEventDispatcher(serviceProvider);

        await domainEventDispatcher.DispatchAsync(new Test1DomainEvent(), cancellationToken);
        await domainEventDispatcher.DispatchAsync(new Test2DomainEvent(), cancellationToken);

        Assert.Equal(2, handler.CalledTimes);

    }

    [Fact]
    public async Task DispatchAsync_DispatchNull_Throws()
    {
        var handler = new MultiHandler();
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var domainEventDispatcher = new DomainEventDispatcher(serviceProvider);

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await domainEventDispatcher.DispatchAsync(null!, cancellationToken));

    }

}
