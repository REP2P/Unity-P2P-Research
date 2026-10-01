using REP2P;
using REP2P.SteamworksNET;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Text = UnityEngine.UI.Text;

public class MMONetworkManager : MonoBehaviour
{
    public Transform localPlayerInstance;
    public GameObject remotePlayerPrefab;
    public GameObject chatBubblePrefab;
    public static GameObject ChatBubblePrefab { get; private set; }
    public static Dictionary<P2PConnection, GameObject> remotePlayers;
    private void Start()
    {
        SteamAPI.Init();
        ChatBubblePrefab = chatBubblePrefab;
        remotePlayers = new Dictionary<P2PConnection, GameObject>();
        P2PNetwork.Initialize(new SteamTransport());
        P2PNetwork.instance.Created += OnCreatedSession;
        P2PNetwork.instance.Joined += OnConnected;
        P2PNetwork.instance.ClientConnected += OnClientConnected;
        P2PNetwork.instance.ClientDisconnected += OnClientDisconnected;
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
                    
                }      
                break;
            case 1:
                var bubble = Instantiate(chatBubblePrefab, remotePlayers[sender].transform.Find("Chatpoint"));
                bubble.GetComponentInChildren<Text>().text = Encoding.UTF8.GetString(packet.packetBytes);
                Destroy(bubble, 5);
                break;
        }
        
    }

    [P2PMessageHandler(0)]
    public static void HandleMovement(P2PConnection sender, P2PMessage message)
    {
        Vector3 position = message.ReadVector3();
        remotePlayers[sender].transform.position = position;
    }

    private void OnClientDisconnected(P2PConnection connection)
    {
        Debug.Log($"{connection.Id} Disconnected!");
        Destroy(remotePlayers[connection]);
        remotePlayers.Remove(connection);
    }

    private void OnClientConnected(P2PConnection connection)
    {
        Debug.Log($"{connection.Id} Connected!");
        var instance = Instantiate(remotePlayerPrefab);
        remotePlayers.Add(connection, instance);
    }

    private void OnApplicationQuit()
    {
        P2PNetwork.Disconnect();
    }

    public void CreateSession()
    {
        P2PNetwork.Disconnect();
        P2PNetwork.CreateSession(64, P2PVisibility.Public);
    }

    private void Update()
    {
        SteamAPI.RunCallbacks();
        P2PNetwork.Poll();
        if(P2PNetwork.instance.IsConnected) SyncPlayerMovement();
    }

    public InputField textChat; 
    public void SendMessageToServer()
    {
        if (textChat.text.Length < 1) return;
        P2PPacket packet = P2PPacket.Create(1, Encoding.UTF8.GetBytes(textChat.text));
        var bubble = Instantiate(chatBubblePrefab, localPlayerInstance.Find("Chatpoint"));
        bubble.GetComponentInChildren<Text>().text = Encoding.UTF8.GetString(packet.packetBytes);
        Destroy(bubble, 5);
        P2PNetwork.SendToAll(packet);
    }

    private void SyncPlayerMovement()
    {
        using (P2PStream stream = P2PStream.Create()) {
            stream.Write(localPlayerInstance.position);
            P2PNetwork.SendToAll(stream.ToPacket(0));
        }
    }
}
