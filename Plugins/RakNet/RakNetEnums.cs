namespace ProjectSMP.Plugins.RakNet
{
    public enum EventType
    {
        IncomingPacket,
        IncomingRPC,
        OutgoingPacket,
        OutgoingRPC,
        IncomingCustomRPC,
        NumberOfEventTypes
    }

    public enum ValueType
    {
        Int8 = 0,
        Int16,
        Int32,
        UInt8,
        UInt16,
        UInt32,
        Float,
        Bool,
        String,
        CInt8,
        CInt16,
        CInt32,
        CUInt8,
        CUInt16,
        CUInt32,
        CFloat,
        CBool,
        CString,
        Bits,
        Float3,
        Float4,
        Vector,
        NormQuat,
        String8,
        String32,
        IgnoreBits
    }

    public enum PacketPriority
    {
        SystemPriority,
        HighPriority,
        MediumPriority,
        LowPriority
    }

    public enum PacketReliability
    {
        Unreliable = 6,
        UnreliableSequenced,
        Reliable,
        ReliableOrdered,
        ReliableSequenced
    }
}