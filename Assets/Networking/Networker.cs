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
            P2PNetwork.Initialize(new FacepunchSteamworksTransport());
            P2PNetwork.messageHandlers.Add(69, HandleMovePacket);
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        string address = "";

        private void OnGUI()
        {
            if(GUILayout.Button("Host")) HostGame();
            address = GUILayout.TextField(address);
            if (GUILayout.Button("Connect")) P2PNetwork.Connect(address);

            P2PNetwork.debug = GUILayout.Toggle(P2PNetwork.debug, "Debug");
        }

        private void OnApplicationQuit() {
            P2PNetwork.Disconnect();
        }

        public void HostGame() {
            P2PNetwork.InitializeSession();
        }
 
        NetDataWriter writer = new();
        private void FixedUpdate() {
            if (P2PNetwork.isConnected) {
                P2PNetwork.Poll();
                writer.Reset();
                writer.Put(localPlayerTransform.position);

                P2PNetwork.SendToAll(69, writer, P2PSendMode.Unreliable);
            }
        }

        public static void HandleMovePacket(P2PConnection sender, int messageId, NetDataReader stream) {
            var pos = stream.GetVector3();

            if (!Instance.networkPlayers.ContainsKey(sender.guid)) {
                Instance.networkPlayers.Add(sender.guid, Instantiate(Instance.networkPlayerPrefab));
                return;
            }

            var player = Instance.networkPlayers[sender.guid];
            player.transform.position = pos;
        }
    }
}