namespace ProjectSMP.Plugins.RakNet
{
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
        IgnoreBits,
        CVector,
        CNetObjectPos,
        String8,
        String32
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