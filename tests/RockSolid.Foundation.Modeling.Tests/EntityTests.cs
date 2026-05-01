namespace RockSolid.Foundation.Modeling.Tests;

using RockSolid.Foundation.Modeling;

public class EntityTests
{
    internal sealed record TestDomainEvent : IDomainEvent<TestDomainEvent>;
    internal sealed class TestEntity(int id) : Entity<TestEntity, int>(id)
    {
        // public void Test()
        // {
        //     RaiseDomainEvent(new TestDomainEvent { });
        // }

        // public void TestNull()
        // {
        //     RaiseDomainEvent(null!);
        // }
    }
    internal sealed class Test1Entity(int id) : Entity<Test1Entity, int>(id);
    internal sealed class Test2Entity(int id) : Entity<Test2Entity, int>(id);

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
    public void CompareEntity_WithNull_AreNotEqual()
    {
        var first = new TestEntity(1);

        bool notEqual = !first.Equals(null);

        Assert.True(notEqual);
    }
    [Fact]
    public void CompareEntity_SameIdDifferentType_AreNotEqual()
    {
        var first = new Test1Entity(1);
        var second = new Test2Entity(1);

        bool notEqual = !first.Equals(second);

        Assert.True(notEqual);
    }

    [Fact]
    public void Transient_WithDefaultId_IsTransient()
    {
        var entity = new TestEntity(default);

        Assert.True(entity.Transient);
    }

    [Fact]
    public void Transient_WithNonDefaultId_IsNotTransient()
    {
        var entity = new TestEntity(1);

        Assert.False(entity.Transient);
    }



}
