using REP2P;
using UnityEngine;
using BitStream = REP2P.BitStream;

public static class BinaryExtensions
{
    public static void Write(this BitStream stream, Vector3 value)
    {
        stream.Write(value.x);
        stream.Write(value.y);
        stream.Write(value.z);
    }
    public static Vector3 ReadVector3(this BitStream stream)
    {
        return new Vector3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
    }
}
