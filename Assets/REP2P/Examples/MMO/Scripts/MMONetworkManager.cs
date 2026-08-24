using REP2P;
using REP2P.Steam;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using BitStream = REP2P.BitStream;
using Text = UnityEngine.UI.Text;

public class MMONetworkManager : MonoBehaviour
{
    public REP2P.Network network;
    public Transform localPlayerInstance;
    public GameObject remotePlayerPrefab;
    public GameObject chatBubblePrefab;
    public Dictionary<NetworkConnection, GameObject> remotePlayers;
    private void Start()
    {
        SteamClient.Init(480);
        remotePlayers = new Dictionary<NetworkConnection, GameObject>();
        network = new REP2P.Network();
        network.Initialize(new SteamLayer());
        network.SubscribePacketHandler(HandlePackets);
        network.OnCreatedSession += OnCreatedSession;
        network.OnConnected += OnConnected;
        network.OnClientConnected += OnClientConnected;
        network.OnClientDisconnected += OnClientDisconnected;
    }

    private void OnCreatedSession()
    {
        Debug.Log("Created Server!");
    }

    private void OnConnected()
    {
        Debug.Log("Connected!");
    }

    private void HandlePackets(NetworkConnection sender, NetworkPacket packet)
    {
        switch (packet.packetId)
        {
            case 0:
                using (BitStream stream = BitStream.Create(packet.packetBytes))
                {
                    Vector3 position = stream.ReadVector3();
                    remotePlayers[sender].transform.position = position;
                }      
                break;
            case 1:
                var bubble = Instantiate(chatBubblePrefab, remotePlayers[sender].transform.Find("Chatpoint"));
                bubble.GetComponentInChildren<Text>().text = Encoding.UTF8.GetString(packet.packetBytes);
                Destroy(bubble, 5);
                break;
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

    public void CreateSession()
    {
        network.Disconnect();
        network.CreateSession(SteamCreateSessionHandle.CreateDefault());
    }

    private void Update()
    {
        SteamClient.RunCallbacks();
        network.Update();
        if(network.Layer.IsConnected())SyncPlayerMovement();
    }

    public InputField textChat; 
    public void SendMessageToServer()
    {
        if (textChat.text.Length < 1) return;
        NetworkPacket packet = NetworkPacket.Create(1, Encoding.UTF8.GetBytes(textChat.text));
        var bubble = Instantiate(chatBubblePrefab, localPlayerInstance.Find("Chatpoint"));
        bubble.GetComponentInChildren<Text>().text = Encoding.UTF8.GetString(packet.packetBytes);
        Destroy(bubble, 5);
        network.SendToAll(packet);
    }

    private void SyncPlayerMovement()
    {
        using (BitStream stream = BitStream.Create()) {
            stream.Write(localPlayerInstance.position);
            NetworkPacket packet = NetworkPacket.Create(0, stream.ToArray());
            network.SendToAll(packet);
        }
    }
}
