namespace RockSolid.Foundation.Modeling.Protobuf;

public static class Extensions
{
    extension(OutboxOptions options)
    {
        public OutboxOptions AddProtobufSerializer()
        {
            return options.AddSerializer<ProtobufOutboxSerializer>();
        }
    }
}
