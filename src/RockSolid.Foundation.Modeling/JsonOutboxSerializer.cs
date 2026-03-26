using System.Text;
using System.Text.Json;

namespace RockSolid.Foundation.Modeling;

public class JsonOutboxSerializer : IOutboxSerializer
{
    public bool TryDeserialize<TOutboxEvent>(ReadOnlySpan<byte> data, out TOutboxEvent? outboxEvent) where TOutboxEvent : IOutboxEvent
    {
        try
        {
            outboxEvent = JsonSerializer.Deserialize<TOutboxEvent>(data);
            return outboxEvent is not null;
        }
        catch (Exception)
        {
            outboxEvent = default;
            return false;
        }
    }

    public bool TrySerialize<TOutboxEvent>(TOutboxEvent outboxEvent, byte[] data) where TOutboxEvent : IOutboxEvent
    {
        try
        {
            var s = JsonSerializer.Serialize(outboxEvent);
            data = Encoding.UTF8.GetBytes(s);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}