using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : NetworkBehaviour
{
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] private GameObject playerPrefab;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (spawnPoints.Count == 0) return;

        int index = NetworkManager.Singleton.ConnectedClients.Count - 1;
        index = Mathf.Clamp(index, 0, spawnPoints.Count - 1);

        Vector3 spawnPos = spawnPoints[index].position;
        Quaternion spawnRot = spawnPoints[index].rotation;

        GameObject player = Instantiate(playerPrefab, spawnPos, spawnRot);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayer;
        }
    }
}