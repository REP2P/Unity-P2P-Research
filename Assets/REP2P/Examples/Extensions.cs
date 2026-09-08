using REP2P;
using UnityEngine;

public static class BinaryExtensions
{
    public static void Write(this P2PStream stream, Vector3 value)
    {
        stream.Write(value.x);
        stream.Write(value.y);
        stream.Write(value.z);
    }
    public static Vector3 ReadVector3(this P2PStream stream)
    {
        return new Vector3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
    }
    public static void Write(this P2PStream stream, Quaternion value)
    {
        stream.Write(value.x);
        stream.Write(value.y);
        stream.Write(value.z);
        stream.Write(value.w);
    }
    public static Quaternion ReadQuaternion(this P2PStream stream)
    {
        return new Quaternion(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
    }
}
