using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DapperDino.UMT.Lobby.Networking
{
    public class GameNetPortal : MonoBehaviour
    {
        public static GameNetPortal Instance => instance;
        private static GameNetPortal instance;

        public event Action OnNetworkReadied;

        public event Action<ConnectStatus> OnConnectionFinished;
        public event Action<ConnectStatus> OnDisconnectReasonReceived;

        public event Action<ulong, int> OnClientSceneChanged;

        public event Action OnUserDisconnectRequested;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            NetworkManager.Singleton.OnServerStarted += HandleNetworkReady;
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnServerStarted -= HandleNetworkReady;
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;

                if (NetworkManager.Singleton.SceneManager != null)
                {
                    NetworkManager.Singleton.SceneManager.OnSceneEvent -= HandleSceneEvent;
                }

                if (NetworkManager.Singleton.CustomMessagingManager == null) { return; }

                UnregisterClientMessageHandlers();
            }
        }

        public void StartHost()
        {
            NetworkManager.Singleton.StartHost();

            RegisterClientMessageHandlers();
        }

        public void RequestDisconnect()
        {
            OnUserDisconnectRequested?.Invoke();
        }

        private void HandleClientConnected(ulong clientId)
        {
            if (clientId != NetworkManager.Singleton.LocalClientId) { return; }

            HandleNetworkReady();
            NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleSceneEvent;
        }

        private void HandleSceneEvent(SceneEvent sceneEvent)
        {
            if (sceneEvent.SceneEventType != SceneEventType.LoadComplete) return;

            OnClientSceneChanged?.Invoke(sceneEvent.ClientId, SceneManager.GetSceneByName(sceneEvent.SceneName).buildIndex);
        }

        private void HandleNetworkReady()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                OnConnectionFinished?.Invoke(ConnectStatus.Success);
            }

            OnNetworkReadied?.Invoke();
        }

        #region Message Handlers

        private void RegisterClientMessageHandlers()
        {
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("ServerToClientConnectResult", (senderClientId, reader) =>
            {
                reader.ReadValueSafe(out ConnectStatus status);
                OnConnectionFinished?.Invoke(status);
            });

            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("ServerToClientSetDisconnectReason", (senderClientId, reader) =>
            {
                reader.ReadValueSafe(out ConnectStatus status);
                OnDisconnectReasonReceived?.Invoke(status);
            });
        }

        private void UnregisterClientMessageHandlers()
        {
            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler("ServerToClientConnectResult");
            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler("ServerToClientSetDisconnectReason");
        }

        #endregion

        #region Message Senders

        public void ServerToClientConnectResult(ulong netId, ConnectStatus status)
        {
            var writer = new FastBufferWriter(sizeof(ConnectStatus), Allocator.Temp);
            writer.WriteValueSafe(status);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("ServerToClientConnectResult", netId, writer);
        }

        public void ServerToClientSetDisconnectReason(ulong netId, ConnectStatus status)
        {
            var writer = new FastBufferWriter(sizeof(ConnectStatus), Allocator.Temp);
            writer.WriteValueSafe(status);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("ServerToClientSetDisconnectReason", netId, writer);
        }

        #endregion
    }
}
