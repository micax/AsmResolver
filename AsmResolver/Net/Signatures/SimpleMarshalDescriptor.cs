namespace AsmResolver.Net.Signatures
{
    public class SimpleMarshalDescriptor : MarshalDescriptor
    {
        private readonly NativeType _nativeType;

        public SimpleMarshalDescriptor(NativeType nativeType)
        {
            _nativeType = nativeType;
        }

        public override NativeType NativeType
        {
            get { return _nativeType; }
        }

        public override uint GetPhysicalLength()
        {
            return sizeof (byte);
        }

        public override void Write(WritingContext context)
        {
            context.Writer.WriteByte((byte)NativeType);
        }
    }
}