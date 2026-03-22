namespace RockSolid.Foundation.Modeling.Tests;

using RockSolid.Foundation.Modeling;

public class EntityTests
{
    internal sealed record TestDomainEvent : IDomainEvent<TestDomainEvent>;
    internal sealed class TestEntity(int id) : Entity<TestEntity, int>(id)
    {
        public void Test()
        {
            RaiseDomainEvent(new TestDomainEvent { });
        }

        public void TestNull()
        {
            RaiseDomainEvent(null!);
        }
    }
    internal sealed class Test1Entity(int id) : Entity<TestEntity, int>(id);
    internal sealed class Test2Entity(int id) : Entity<TestEntity, int>(id);

    [Fact]
    public void CompareEntity_SameId_AreEqual()
    {
        var first = new TestEntity(1);
        var second = new TestEntity(1);

        bool equal = first.Equals(second)
            && first == second
            && first.GetHashCode() == second.GetHashCode();

        Assert.True(equal);
    }

    [Fact]
    public void CompareEntity_DifferentId_AreNotEqual()
    {
        var first = new TestEntity(1);
        var second = new TestEntity(2);

        bool notEqual = !first.Equals(second) && first != second;

        Assert.True(notEqual);
    }

    [Fact]
    public void CompareEntity_SameIdDifferentType_AreNotEqual()
    {
        var first = new Test1Entity(1);
        var second = new Test2Entity(1);

        bool notEqual = !first.Equals(second) && first != second;

        Assert.True(notEqual);
    }


    [Fact]
    public void RaiseDomainEvent_ShouldAddToList()
    {
        var entity = new TestEntity(1);

        entity.Test();

        Assert.Single(entity.DomainEvents);
    }

    [Fact]
    public void RaiseDomainEvent_WithNull_ShouldThrow()
    {
        var entity = new TestEntity(1);

        Assert.Throws<ArgumentNullException>(() => entity.TestNull());
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmptyList()
    {
        var entity = new TestEntity(1);
        entity.Test();

        entity.ClearDomainEvents();

        Assert.Empty(entity.DomainEvents);
    }

}
