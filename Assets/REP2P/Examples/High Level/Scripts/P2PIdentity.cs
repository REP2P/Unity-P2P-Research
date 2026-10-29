using LiteNetLib.Utils;
using REP2P;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class P2PIdentity : MonoBehaviour
{
    public int netId { get; internal set; }
    public byte nextRpc { get; internal set; }
    private static Dictionary<byte, P2PRPC> rpcs = new();
    public P2PObservable observable { get; set; }
    [SerializeField] private MonoBehaviour observableComponent;
    private void Start()
    {
        observable = observableComponent as P2PObservable;
        observable ??= GetComponent<P2PObservable>();
    }
    public byte RegisterRpc(P2PRPC rpcHandler)
    {
        byte rpc = nextRpc;
        rpcs.Add(rpc, rpcHandler);
        nextRpc++;
        return rpc;
    }

    public void SerializeIdentity()
    {
        NetDataWriter stream = new NetDataWriter();
        observable.OnP2PSerialize(stream, new() { time = P2PNetworkManager.GetNetworkTime() });
        // TODO: send it
    }

    public void SendRpc(byte rpc, P2PConnection connection, NetDataWriter stream, P2PSendMode send = P2PSendMode.Unreliable)
    { 
        P2PNetworkManager.SendRpc(netId, connection, rpc, stream, send);
    }
    public void SendRpc(byte rpc, P2PRPCReceivers receivers, NetDataWriter stream, P2PSendMode send = P2PSendMode.Unreliable)
    {
        switch (receivers)
        {
            case P2PRPCReceivers.All:
                rpcs[rpc].Invoke(P2PNetwork.connection, new NetDataReader(stream.Data));
                break;
        }
        P2PNetworkManager.SendRpc(netId, rpc, stream, send);
    }


    internal void OnRpcReceived(P2PConnection sender, byte rpc, byte[] data)
    {
        rpcs[rpc].Invoke(sender, new NetDataReader(data));
    }
}
