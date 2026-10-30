using REP2P;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct P2PPacketInfo
{
    public double time;
    public P2PConnection connection;
    public bool isHost => connection.isHost;
    public bool isMe;
}
