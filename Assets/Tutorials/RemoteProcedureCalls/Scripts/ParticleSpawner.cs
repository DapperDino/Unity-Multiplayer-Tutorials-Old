using Unity.Netcode;
using UnityEngine;

namespace DapperDino.UMT.RemoteProcedureCalls
{
    public class ParticleSpawner : NetworkBehaviour
    {
        [SerializeField] private GameObject particlePrefab;

        private void Update()
        {
            // Make sure this is belongs to us
            if (!IsOwner) { return; }

            // Check to see if we just hit the space key
            if (!Input.GetKeyDown(KeyCode.Space)) { return; }

            // Send a message to THE server to execute this method
            SpawnParticleServerRpc();

            // Spawn it instantly for ourself since we don't need to
            // wait for the server
            Instantiate(particlePrefab, transform.position, Quaternion.identity);
        }

        [ServerRpc(Delivery = RpcDelivery.Unreliable)]
        private void SpawnParticleServerRpc()
        {
            // Send a message to ALL clients to execute this method
            SpawnParticleClientRpc();
        }

        [ClientRpc(Delivery = RpcDelivery.Unreliable)]
        private void SpawnParticleClientRpc()
        {
            // Make sure we don't spawn it twice for ourselves
            if (IsOwner) { return; }

            // Spawn the particles!
            Instantiate(particlePrefab, transform.position, Quaternion.identity);
        }
    }
}
