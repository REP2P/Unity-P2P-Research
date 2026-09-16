using REP2P;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P2PTransform : MonoBehaviour, P2PObservable
{
    public bool syncPosition = true;
    public bool syncRotation = true;
    public bool syncScale = false;

    private Vector3 nextPosition;
    private Quaternion nextRotation;
    private Vector3 nextScale;
    private double lastTimestamp;
    public float lerpSpeed = 10f;
    public float teleportThreshold = 5f;

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, nextPosition, lerpSpeed * Time.deltaTime);
    }

    public void OnP2PSerialize(P2PStream stream, bool write, P2PMessageInfo info)
    {
        if (write)
        {
            if (syncPosition) stream.Write(transform.position);
            if (syncRotation) stream.Write(transform.rotation);
            if (syncScale) stream.Write(transform.localScale);
        }
        else
        {
            double receivedTimestamp = info.time;
            if (receivedTimestamp < lastTimestamp || !info.isHost) return;
            lastTimestamp = receivedTimestamp;
            nextPosition = stream.ReadVector3();
            nextRotation = stream.ReadQuaternion();
            nextScale = stream.ReadVector3();
        }      
    }
}