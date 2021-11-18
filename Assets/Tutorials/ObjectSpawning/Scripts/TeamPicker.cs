using Unity.Netcode;
using UnityEngine;

namespace DapperDino.UMT.ObjectSpawning
{
    public class TeamPicker : MonoBehaviour
    {
        public void SelectTeam(int teamIndex)
        {
            // Get the local client's id
            ulong localClientId = NetworkManager.Singleton.LocalClientId;

            // Try to get the local client object
            // Return if unsuccessful
            if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(localClientId, out NetworkClient networkClient))
            {
                return;
            }

            // Try to get the TeamPlayer component from the player object
            // Return if unsuccessful
            if (!networkClient.PlayerObject.TryGetComponent<TeamPlayer>(out var teamPlayer))
            {
                return;
            }

            // Send a message to the server to set the local client's team
            teamPlayer.SetTeamServerRpc((byte)teamIndex);
        }
    }
}
