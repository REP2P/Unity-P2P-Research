using REP2P;
using REP2P.Steam;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public REP2P.Network network;
    public Transform localPlayerInstance;
    public GameObject remotePlayerPrefab;
    public Dictionary<NetworkConnection, GameObject> remotePlayers;
    private void Start()
    {
        SteamClient.Init(480);
        remotePlayers = new Dictionary<NetworkConnection, GameObject>();
        network = new REP2P.Network();
        network.Initialize(new SteamLayer());
        network.SubscribePacketHandler(HandlePackets);
        network.OnConnected += OnConnected;
        network.OnClientConnected += OnClientConnected;
        network.OnClientDisconnected += OnClientDisconnected;
    }

    private void OnConnected()
    {
        Debug.Log("Connected!");
    }

    private void HandlePackets(NetworkConnection sender, NetworkPacket message)
    {
        using (MemoryStream ms = new MemoryStream(message.packetBytes))
        {
            using (BinaryReader reader = new BinaryReader(ms))
            {
                switch (message.packetId)
                {
                    case 0:
                        Vector3 position = reader.ReadVector3();
                        remotePlayers[sender].transform.position = position;
                        break;
                    case 1:

                        break;
                }
            }         
        }
    }

    private void OnClientDisconnected(NetworkConnection connection)
    {
        Debug.Log($"{connection.connectionId} Disconnected!");
        Destroy(remotePlayers[connection]);
        remotePlayers.Remove(connection);
    }

    private void OnClientConnected(NetworkConnection connection)
    {
        Debug.Log($"{connection.connectionId} Connected!");
        var instance = Instantiate(remotePlayerPrefab);
        remotePlayers.Add(connection, instance);
    }

    private void OnApplicationQuit()
    {
        network.DeInitialize();
        SteamClient.Shutdown();
    }

    public void CreateServer()
    {
        network.CreateServer(SteamCreateServer.CreateDefault());
    }

    private void Update()
    {
        network.Update();
        if(network.Layer.IsConnected())SyncPlayerMovement();
    }

    private void SyncPlayerMovement()
    {
        using (MemoryStream ms = new MemoryStream()) {
            using (BinaryWriter writer = new BinaryWriter(ms)) {
                writer.Write(localPlayerInstance.position);
                NetworkPacket packet = new NetworkPacket(0, ms.ToArray());
                network.SendToAll(packet);
            }
        }
    }
}
