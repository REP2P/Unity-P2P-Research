using LiteNetLib.Utils;
using REP2P;
using REP2P.Facepunch.Steamworks;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Networking
{
    public class Networker : MonoBehaviour
    {
        public static Networker Instance;
        public Dictionary<Guid, PlayerNetwork> networkPlayers = new();
        public PlayerNetwork networkPlayerPrefab;
        public Transform localPlayerTransform;
        private void Start() {
            if (Instance) {
                Destroy(gameObject);
                return;
            }
            SteamClient.Init(480);
            P2PLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError);
            SteamMatchmaking.OnLobbyEntered += SteamMatchmaking_OnLobbyEntered;
            P2PNetwork.Initialize(new P2PFacepunchService());
            P2PNetwork.clientConnected += (connection) =>
            {
                if (!networkPlayers.ContainsKey(connection.guid))
                {
                    var player = Instantiate(networkPlayerPrefab);
                    networkPlayers.Add(connection.guid, player);
                }
            };
            P2PNetwork.clientDisconnected += P2PNetwork_clientDisconnected;
            //P2PNetwork.
            P2PNetwork.messageHandlers.Add(69, HandleMovePacket);
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void P2PNetwork_clientDisconnected(P2PConnection obj)
        {
            if (networkPlayers.ContainsKey(obj.guid))
            {
                Destroy(networkPlayers[obj.guid].gameObject);
                networkPlayers.Remove(obj.guid);
            }
        }

        private void SteamMatchmaking_OnLobbyEntered(Steamworks.Data.Lobby obj)
        {
            lobbyId = obj.Id.Value.ToString();
        }

        string address = "";
        string lobbyId;

        private void OnGUI()
        {
            if(GUILayout.Button("Host")) HostGame();
            address = GUILayout.TextField(address);
            if (GUILayout.Button("Connect")) P2PNetwork.JoinRoom(address);

            GUILayout.TextField(lobbyId);
            GUILayout.Label("NetId" + netInstanceId);
            P2PNetwork.debug = GUILayout.Toggle(P2PNetwork.debug, "Debug");

            if (GUILayout.Button("Disconnect")) P2PNetwork.Disconnect();
        }

        private void OnApplicationQuit() {
            P2PNetwork.Disconnect();
        }

        public void HostGame() {
            P2PNetwork.CreateRoom();
        }
        private string netInstanceId;
        NetDataWriter writer = new();
        private void FixedUpdate() {
            if (P2PNetwork.isHost)
            {
                netInstanceId = GetInstanceID().ToString();
                P2PNetwork.SetRoomData("net", netInstanceId);
            }
            else
            {
                netInstanceId = P2PNetwork.GetRoomData("net");
            }
            if (P2PNetwork.isConnected) {
                P2PNetwork.Poll();
                writer.Reset();
                writer.Put(localPlayerTransform.position);

                P2PNetwork.SendToAll(69, writer, P2PSendMode.Unreliable);
            }
        }

        public static void HandleMovePacket(P2PConnection sender, int messageId, NetDataReader stream) {
            if (!Instance.networkPlayers.ContainsKey(sender.guid))
            {
                return;
            }
            var pos = stream.GetVector3();
            var player = Instance.networkPlayers[sender.guid];
            player.transform.position = pos;
        }
    }
}