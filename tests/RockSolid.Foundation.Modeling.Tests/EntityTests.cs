using System.Runtime.CompilerServices;

namespace RockSolid.Foundation.Modeling.Tests;

public class EntityIdTests
{
    internal sealed record TestId(int Value) : IEntityId<TestId, int>;

    [Fact]
    public void CompareTo()
    {
        var a = new TestId(1);
        var b = new TestId(2);
        Assert.True(((IComparable<TestId>)a).CompareTo(b) < 0);
        Assert.True(((IComparable<TestId>)b).CompareTo(a) > 0);
        Assert.True(((IComparable<TestId>)a).CompareTo(a) == 0);
        Assert.True(((IComparable<TestId>)b).CompareTo(b) == 0);
        Assert.True(((IComparable<TestId>)a).CompareTo(null) > 0);
    }
}

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
    public void Transient_WithDefaultId_ReturnsTrue()
    {
        var entity = new TestEntity(default);

        Assert.True(entity.Transient);
    }

    [Fact]
    public void Transient_WithNonDefaultId_ReturnsFalse()
    {
        var entity = new TestEntity(1);

        Assert.False(entity.Transient);
    }

    [Fact]
    public void GetHashCode_WhenTransient_ReturnsInstanceHashCode()
    {
        var entity1 = new TestEntity(default);
        var entity2 = new TestEntity(default);

        Assert.NotEqual(entity1.GetHashCode(), entity2.GetHashCode());

        int hash1 = entity1.GetHashCode();
        int hash2 = entity1.GetHashCode();
        Assert.Equal(hash1, hash2);

        Assert.NotEqual(entity1, entity2);
    }

}
