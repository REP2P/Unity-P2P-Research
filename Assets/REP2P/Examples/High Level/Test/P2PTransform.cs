using LiteNetLib.Utils;
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
        transform.rotation = Quaternion.Lerp(transform.rotation, nextRotation, lerpSpeed * Time.deltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, nextScale, lerpSpeed * Time.deltaTime);
    }

    public void OnP2PSerialize(NetDataWriter writer, P2PPacketInfo info)
    {
        if (syncPosition) writer.Put(transform.position);
        if (syncRotation) writer.Put(transform.rotation);
        if (syncScale) writer.Put(transform.localScale);
    }

    public void OnP2PDeserialize(NetDataReader reader, P2PPacketInfo info)
    {
        double receivedTimestamp = info.time;
        if (receivedTimestamp < lastTimestamp || !info.isHost) return;
        lastTimestamp = receivedTimestamp;
        if (syncPosition) nextPosition = reader.GetVector3();
        if (syncRotation) nextRotation = reader.GetQuaternion();
        if (syncScale) nextScale = reader.GetVector3();
    }
}