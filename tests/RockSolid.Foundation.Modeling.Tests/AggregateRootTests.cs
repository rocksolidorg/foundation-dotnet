namespace RockSolid.Foundation.Modeling.Tests;

using RockSolid.Foundation.Modeling;

public class AggregateRootTests
{

    internal sealed record TestEvent : IDomainEvent<TestEvent>;
    internal sealed class TestAggregate(int id) : AggregateRoot<TestAggregate, int>(id)
    {
        public void Test()
        {
            RaiseDomainEvent(new TestEvent());
        }

        public void TestNull()
        {
            RaiseDomainEvent(null!);
        }
    }

    // [Fact]
    // public void Aggregate_Modify_ShouldChangeVersion()
    // {
    //     var agg = new TestAggregate(1);
    //     agg.Modify();
    //     Assert.Equal(1, agg.Version);
    // }


    [Fact]
    public void RaiseDomainEvent_ShouldAddToList()
    {
        var entity = new TestAggregate(1);

        entity.Test();

        Assert.Single(entity.DomainEvents);
    }

    [Fact]
    public void RaiseDomainEvent_WithNull_ShouldThrow()
    {
        var entity = new TestAggregate(1);

        Assert.Throws<ArgumentNullException>(() => entity.TestNull());
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmptyList()
    {
        var entity = new TestAggregate(1);
        entity.Test();

        entity.ClearDomainEvents();

        Assert.Empty(entity.DomainEvents);
    }

}