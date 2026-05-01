namespace RockSolid.Foundation.Modeling;

public interface IOutboxSerializer
{
    bool TrySerialize<TOutboxEvent>(TOutboxEvent outboxEvent, byte[] data)
        where TOutboxEvent : IOutboxEvent;

    bool TryDeserialize<TOutboxEvent>(ReadOnlySpan<byte> data, out TOutboxEvent? outboxEvent)
        where TOutboxEvent : IOutboxEvent;
}
