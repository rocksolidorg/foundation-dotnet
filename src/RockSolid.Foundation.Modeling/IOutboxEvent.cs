namespace RockSolid.Foundation.Modeling;

public interface IOutboxEvent : IDomainEvent;

public interface IOutboxEvent<TSelf> : IOutboxEvent, IDomainEvent<TSelf>
    where TSelf : IOutboxEvent<TSelf>;


public class OutboxOptions
{
    public List<IOutboxSerializer> Serializers = [
        new JsonOutboxSerializer()
    ];

    public OutboxOptions AddSerializer<TSerializer>()
        where TSerializer : IOutboxSerializer, new()
    {
        Serializers.Add(new TSerializer());
        return this;
    }

    public OutboxOptions AddJsonSerializer()
        => AddSerializer<JsonOutboxSerializer>();

    public OutboxOptions ClearSerializers()
    {
        Serializers.Clear();
        return this;
    }



}