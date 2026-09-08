using REP2P;
using System;
using System.Collections.Generic;
using UnityEngine;

public class P2PIdentity : MonoBehaviour
{
    public int netId { get; internal set; }
    public byte nextRpc { get; internal set; }
    private static Dictionary<byte, P2PRPC> rpcs = new();
    public byte RegisterRpc(P2PRPC rpcHandler)
    {
        byte rpc = nextRpc;
        rpcs.Add(rpc, rpcHandler);
        nextRpc++;
        return rpc;
    }

    public void SendRpc(byte rpc, P2PConnection connection, P2PStream stream, P2PSend send = P2PSend.Unreliable)
    { 
        P2PNetworkManager.SendRpc(netId, connection, rpc, stream, send);
    }
    public void SendRpc(byte rpc, P2PRPCReceivers receivers, P2PStream stream, P2PSend send = P2PSend.Unreliable)
    {
        switch (receivers)
        {
            case P2PRPCReceivers.All:
                // Make sure to add a method that returns my connection too!
                rpcs[rpc].Invoke(null, stream);
                break;
        }
        P2PNetworkManager.SendRpc(netId, rpc, stream, send);
    }


    internal void OnRpcReceived(P2PConnection sender, byte rpc, byte[] data)
    {
        rpcs[rpc].Invoke(sender, P2PStream.Create(data));
    }
}
