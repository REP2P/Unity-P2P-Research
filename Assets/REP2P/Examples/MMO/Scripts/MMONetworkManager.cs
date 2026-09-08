using REP2P;
using REP2P.Steam;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Text = UnityEngine.UI.Text;

public class MMONetworkManager : MonoBehaviour
{
    public P2PNetwork network;
    public Transform localPlayerInstance;
    public GameObject remotePlayerPrefab;
    public GameObject chatBubblePrefab;
    public Dictionary<P2PConnection, GameObject> remotePlayers;
    private void Start()
    {
        SteamClient.Init(480);
        remotePlayers = new Dictionary<P2PConnection, GameObject>();
        network = new P2PNetwork();
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

    private void HandlePackets(P2PConnection sender, P2PPacket packet)
    {
        switch (packet.packetId)
        {
            case 0:
                using (P2PStream stream = P2PStream.Create(packet.packetBytes))
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

    private void OnClientDisconnected(P2PConnection connection)
    {
        Debug.Log($"{connection.connectionId} Disconnected!");
        Destroy(remotePlayers[connection]);
        remotePlayers.Remove(connection);
    }

    private void OnClientConnected(P2PConnection connection)
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
        P2PPacket packet = P2PPacket.Create(1, Encoding.UTF8.GetBytes(textChat.text));
        var bubble = Instantiate(chatBubblePrefab, localPlayerInstance.Find("Chatpoint"));
        bubble.GetComponentInChildren<Text>().text = Encoding.UTF8.GetString(packet.packetBytes);
        Destroy(bubble, 5);
        network.SendToAll(packet);
    }

    private void SyncPlayerMovement()
    {
        using (P2PStream stream = P2PStream.Create()) {
            stream.Write(localPlayerInstance.position);
            network.SendToAll(stream.ToPacket(0));
        }
    }
}
