using System.Collections.Concurrent;
using Google.Protobuf;

namespace RockSolid.Foundation.Modeling.Protobuf;

internal sealed class ProtobufOutboxSerializer : IOutboxSerializer
{

    private delegate IOutboxEvent? Delegate(ReadOnlySpan<byte> data);
    private static readonly ConcurrentDictionary<Type, Delegate> _cache = [];

    private static IOutboxEvent? TryDeserializeCore<TOutboxEvent>(ReadOnlySpan<byte> data)
        where TOutboxEvent : IOutboxEvent, IMessage, new()
    {
        var temp = new TOutboxEvent();
        temp.MergeFrom(data);
        return temp;
    }

    public bool TryDeserialize<TOutboxEvent>(ReadOnlySpan<byte> data, out TOutboxEvent? outboxEvent)
        where TOutboxEvent : IOutboxEvent
    {
        var tryDeserialize = _cache.GetOrAdd(typeof(TOutboxEvent),
            static type => typeof(ProtobufOutboxSerializer)
                .GetMethod(nameof(TryDeserializeCore), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(type)
                .CreateDelegate<Delegate>()
        );
        if (tryDeserialize(data) is TOutboxEvent t)
        {
            outboxEvent = t;
            return true;
        }
        else
        {
            outboxEvent = default;
            return false;
        }

    }

    public bool TrySerialize<TOutboxEvent>(TOutboxEvent outboxEvent, byte[] data)
        where TOutboxEvent : IOutboxEvent
    {
        try
        {
            if (outboxEvent is IMessage imessage)
            {
                data = imessage.ToByteArray();
                return true;
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
}