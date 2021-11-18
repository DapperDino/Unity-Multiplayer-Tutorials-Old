using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Text;
using System.Collections.Generic;

namespace DapperDino.UMT.PlayerNames
{
    public class PasswordNetworkManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField passwordInputField;
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private GameObject teamPickerUI;
        [SerializeField] private GameObject passwordEntryUI;
        [SerializeField] private GameObject leaveButton;

        private static Dictionary<ulong, PlayerData> clientData;

        private void Start()
        {
            NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        }

        private void OnDestroy()
        {
            // Prevent error in the editor
            if (NetworkManager.Singleton == null) { return; }

            NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        }

        public void Host()
        {
            clientData = new Dictionary<ulong, PlayerData>();
            clientData[NetworkManager.Singleton.LocalClientId] = new PlayerData(nameInputField.text);

            // Hook up password approval check
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.StartHost();
        }

        public void Client()
        {
            var payload = JsonUtility.ToJson(new ConnectionPayload()
            {
                password = passwordInputField.text,
                playerName = nameInputField.text
            });

            byte[] payloadBytes = Encoding.ASCII.GetBytes(payload);

            // Set password ready to send to the server to validate
            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
            NetworkManager.Singleton.StartClient();
        }

        public void Leave()
        {
            NetworkManager.Singleton.Shutdown();

            if (NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
            }

            passwordEntryUI.SetActive(true);
            leaveButton.SetActive(false);
            teamPickerUI.SetActive(false);
        }

        public static PlayerData? GetPlayerData(ulong clientId)
        {
            if (clientData.TryGetValue(clientId, out PlayerData playerData))
            {
                return playerData;
            }

            return null;
        }

        private void HandleServerStarted()
        {
            // Temporary workaround to treat host as client
            if (NetworkManager.Singleton.IsHost)
            {
                HandleClientConnected(NetworkManager.Singleton.ServerClientId);
            }
        }

        private void HandleClientConnected(ulong clientId)
        {
            // Are we the client that is connecting?
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                passwordEntryUI.SetActive(false);
                leaveButton.SetActive(true);
                teamPickerUI.SetActive(true);
            }
        }

        private void HandleClientDisconnect(ulong clientId)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                clientData.Remove(clientId);
            }

            // Are we the client that is disconnecting?
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                passwordEntryUI.SetActive(true);
                leaveButton.SetActive(false);
                teamPickerUI.SetActive(false);
            }
        }

        private void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
        {
            string payload = Encoding.ASCII.GetString(connectionData);
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

            bool approveConnection = connectionPayload.password == passwordInputField.text;

            Vector3 spawnPos = Vector3.zero;
            Quaternion spawnRot = Quaternion.identity;

            if (approveConnection)
            {
                switch (NetworkManager.Singleton.ConnectedClients.Count)
                {
                    case 1:
                        spawnPos = new Vector3(0f, 0f, 0f);
                        spawnRot = Quaternion.Euler(0f, 180f, 0f);
                        break;
                    case 2:
                        spawnPos = new Vector3(2f, 0f, 0f);
                        spawnRot = Quaternion.Euler(0f, 225, 0f);
                        break;
                }

                clientData[clientId] = new PlayerData(connectionPayload.playerName);
            }

            callback(true, null, approveConnection, spawnPos, spawnRot);
        }
    }
}
