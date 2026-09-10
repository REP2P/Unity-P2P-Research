using REP2P;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P2PTransform : MonoBehaviour
{
    private bool maySend => P2PNetworkManager.network.IsHost;
    public P2PIdentity identity;

    public bool syncPosition = true;
    public bool syncRotation = true;
    public bool syncScale = false;

    private Vector3 nextPosition;
    private Quaternion nextRotation;
    private Vector3 nextScale;
    private float lastTimestamp;
    public float lerpSpeed = 10f;
    public float teleportThreshold = 5f;

    // RPCS
    private byte RPC_Transform;

    private void Start()
    {
        identity ??= GetComponentInParent<P2PIdentity>();
        RPC_Transform = identity.RegisterRpc(OnTransformReceived);
    }

    private void OnTransformReceived(P2PConnection sender, P2PStream stream)
    {
        double receivedTimestamp = stream.ReadDouble();
        if (receivedTimestamp < lastTimestamp || !sender.isHost) return;
        nextPosition = stream.ReadVector3();
        nextRotation = stream.ReadQuaternion();
        nextScale = stream.ReadVector3();
    }

    private void Update()
    {
        if (maySend)
        {
            using (P2PStream stream = P2PStream.Create())
            {
                stream.Write(P2PNetworkManager.GetNetworkTime());
                if (syncPosition) stream.Write(transform.position);
                if (syncRotation) stream.Write(transform.rotation);
                if (syncScale) stream.Write(transform.localScale);
                identity.SendRpc(RPC_Transform, P2PRPCReceivers.Others, stream, P2PSend.Unreliable);
            }               
        }
        else
        {

        }
    }
}