using MLAPI;
using MLAPI.NetworkVariable;
using TMPro;
using UnityEngine;

namespace DapperDino.UMT.PlayerNames
{
    public class PlayerOverheadDisplay : NetworkBehaviour
    {
        [SerializeField] private TMP_Text displayNameText;

        private NetworkVariableString displayName = new NetworkVariableString();

        public override void NetworkStart()
        {
            if (!IsServer) { return; }

            PlayerData? playerData = PasswordNetworkManager.GetPlayerData(OwnerClientId);

            if(playerData.HasValue)
            {
                displayName.Value = playerData.Value.PlayerName;
            }
        }

        private void OnEnable()
        {
            displayName.OnValueChanged += HandleDisplayNameChanged;
        }

        private void OnDisable()
        {
            displayName.OnValueChanged -= HandleDisplayNameChanged;
        }

        private void HandleDisplayNameChanged(string oldDisplayName, string newDisplayName)
        {
            displayNameText.text = newDisplayName;
        }
    }
}
