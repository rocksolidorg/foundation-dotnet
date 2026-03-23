// namespace RockSolid.Foundation.Modeling.Tests;

// using RockSolid.Foundation.Modeling;

// public class AggregateRootTests
// {
//     internal sealed record TestEvent : IDomainEvent<TestEvent>;
//     internal sealed class TestAggregate(int id) : AggregateRoot<TestAggregate, int>(id)
//     {
//         public void Modify()
//         {
//             ++Version;
//         }
//     }

//     [Fact]
//     public void Aggregate_Modify_ShouldChangeVersion()
//     {
//         var agg = new TestAggregate(1);
//         agg.Modify();
//         Assert.Equal(1, agg.Version);
//     }
// }