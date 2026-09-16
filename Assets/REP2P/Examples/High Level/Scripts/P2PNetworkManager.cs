using REP2P;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;

public static class P2PNetworkManager
{
    public static int nextNetId { get; internal set; } = 0;
    public static Dictionary<int, P2PIdentity> identities { get; internal set; } = new Dictionary<int, P2PIdentity>();
    [RuntimeInitializeOnLoadMethod]
    private static void OnStart()
    {
        P2PNetwork.SubscribePacketHandler(ReceivedPacket);
        var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
        playerLoop.updateDelegate += () =>
        {
            P2PNetwork.Update();
        };
        PlayerLoop.SetPlayerLoop(playerLoop);
    }
    private static readonly long s_timeInitialized = Environment.TickCount;

    public static double GetNetworkTime()
    {
        return (double)((uint)Environment.TickCount - s_timeInitialized) / 1000.0;
    }
    private static void ReceivedPacket(P2PConnection sender, P2PPacket message)
    {
        switch((P2PHLPackets)message.packetId)
        {
            case P2PHLPackets.Rpc:
                using(P2PStream stream = P2PStream.Create(message.packetBytes))
                {
                    int netId = stream.ReadInt();
                    byte rpc = stream.ReadByte();
                    byte[] data = stream.ReadBytes();
                    if(identities.TryGetValue(netId, out P2PIdentity identity))
                    {
                        identity.OnRpcReceived(sender, rpc, data);
                    }
                }
                break;
        }
    }

    public static void Initialize(P2PLayer layer)
    {
        P2PNetwork.Initialize(layer);
    }
    public static void DeInitialize()
    {
        P2PNetwork.DeInitialize();
    }
    public static void ResetScene()
    {
        nextNetId = 0;
        identities.Clear();
        foreach(var identity in P2PScene.instance.identities)
        {
            RegisterIdentity(identity);
        }
    }
    public static void RegisterIdentity(P2PIdentity identity)
    {
        identity.netId = nextNetId;
        identities.Add(nextNetId, identity);
        nextNetId++;
    }

    public static void SendRpc(int netId, P2PConnection connection, byte rpc, P2PStream stream, P2PSend send)
    {
        P2PNetwork.Send(connection, CreateRpcPacket(netId, rpc, stream), send);
    }
    private static P2PPacket CreateRpcPacket(int netId, byte rpc, P2PStream stream)
    {
        using (P2PStream rpcStream = P2PStream.Create())
        {
            rpcStream.Write(netId);
            rpcStream.Write(rpc);
            rpcStream.Write(stream.ToArray());
            return rpcStream.ToPacket((int)P2PHLPackets.Rpc);
        }
    }
    public static void SendRpc(int netId, byte rpc, P2PStream stream, P2PSend send)
    {
        P2PNetwork.SendToAll(CreateRpcPacket(netId, rpc, stream), send);
    }
}
