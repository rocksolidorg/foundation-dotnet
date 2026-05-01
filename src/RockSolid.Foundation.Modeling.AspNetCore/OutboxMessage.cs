namespace RockSolid.Foundation.Modeling.AspNetCore;

public sealed class OutboxMessage(int index, Guid id, string format, DateTimeOffset sentAt, byte[] data) : AggregateRoot<OutboxMessage, Guid>(id)
{
    public int Index { get; } = index;
    public string Format { get; } = format;
    public DateTimeOffset SentAt { get; } = sentAt;
    public byte[] Data { get; } = data;
}
