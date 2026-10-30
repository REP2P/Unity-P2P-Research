using LiteNetLib.Utils;
using REP2P;
using UnityEngine;

public static class BinaryExtensions
{
    public static void Put(this NetDataWriter writer, Vector3 value)
    {
        writer.Put(value.x);
        writer.Put(value.y);
        writer.Put(value.z);
    }
    public static Vector3 GetVector3(this NetDataReader reader)
    {
        return new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
    }
    public static void Put(this NetDataWriter writer, Quaternion value)
    {
        writer.Put(value.x);
        writer.Put(value.y);
        writer.Put(value.z);
        writer.Put(value.w);
    }
    public static Quaternion GetQuaternion(this NetDataReader reader)
    {
        return new Quaternion(reader.GetFloat(), reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
    }
}
