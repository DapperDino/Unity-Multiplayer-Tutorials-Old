using Unity.Netcode;
using UnityEngine;

namespace DapperDino.UMT.ObjectSpawning
{
    public class BallSpawner : NetworkBehaviour
    {
        [SerializeField] private NetworkObject ballPrefab;

        private Camera mainCamera;

        private void Start()
        {
            // Cache a reference to the main camera
            mainCamera = Camera.main;
        }

        private void Update()
        {
            // Make sure this is belongs to us
            if (!IsOwner) { return; }

            // Check to see if we just hit the left mouse button
            if (!Input.GetMouseButtonDown(0)) { return; }

            // Find where we clicked in world space
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) { return; }

            // Send a message to the server to execute this method
            SpawnBallServerRpc(hit.point);
        }

        [ServerRpc]
        private void SpawnBallServerRpc(Vector3 spawnPos)
        {
            // Spawn the prefab in normally (on the server)
            NetworkObject ballInstance = Instantiate(ballPrefab, spawnPos, Quaternion.identity);

            // Replicate the object to all clients and give
            // ownership to the client that owns this player
            ballInstance.SpawnWithOwnership(OwnerClientId);
        }
    }
}
