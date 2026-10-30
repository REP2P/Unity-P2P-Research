using LiteNetLib.Utils;
using REP2P;
using REP2P.Facepunch.Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using UnityEngine;
using UnityEngine.LowLevel;

public static class P2PNetworkManager
{
    public static int nextNetId { get; internal set; } = 0;
    public static Dictionary<int, P2PIdentity> identities { get; internal set; } = new Dictionary<int, P2PIdentity>();
    [RuntimeInitializeOnLoadMethod]
    private static void OnStart()
    {
        Initialize(new FacepunchSteamworksTransport());
        var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
        playerLoop.updateDelegate += () =>
        {
            P2PNetwork.Poll();
        };
        PlayerLoop.SetPlayerLoop(playerLoop);
        P2PNetwork.messageHandlers.Add((int)P2PHLPackets.Rpc, OnRpcReceived);
    }

    private static void OnRpcReceived(P2PConnection sender, int messageId, NetDataReader message)
    {
        int netId = message.GetInt();
        byte rpc = message.GetByte();
        byte[] data = message.GetRemainingBytes();
        if (identities.TryGetValue(netId, out P2PIdentity identity))
        {
            identity.OnRpcReceived(sender, rpc, data);
        }
    }

    private static readonly long s_timeInitialized = Environment.TickCount;

    public static double GetNetworkTime()
    {
        return (double)((uint)Environment.TickCount - s_timeInitialized) / 1000.0;
    }

    public static void Initialize(P2PTransport transport)
    {
        P2PNetwork.Initialize(transport);
    }
    public static void DeInitialize()
    {
        P2PNetwork.Disconnect();
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

    public static void SendRpc(int netId, P2PConnection connection, byte rpc, NetDataWriter stream, P2PSendMode send)
    {
        P2PNetwork.Send(connection, (int)P2PHLPackets.Rpc, CreateRpcMessage(netId, rpc, stream), send);
    }
    private static NetDataWriter CreateRpcMessage(int netId, byte rpc, NetDataWriter stream)
    {
        NetDataWriter rpcStream = new NetDataWriter();
        rpcStream.Put(netId);
        rpcStream.Put(rpc);
        rpcStream.Put(stream.Data);
        return rpcStream;
    }
    public static void SendRpc(int netId, byte rpc, NetDataWriter stream, P2PSendMode send)
    {
        P2PNetwork.SendToAll((int)P2PHLPackets.Rpc,CreateRpcMessage(netId, rpc, stream), send);
    }
}
