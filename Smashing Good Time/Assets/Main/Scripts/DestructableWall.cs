using Unity.Netcode;
using UnityEngine;

public class DestructableWall : NetworkBehaviour
{
    [Header("Impact Settings")]
    public float requiredImpact = 5f;
    public float requiredHealth = 50f;

    // Wall that replaces
    [SerializeField] private GameObject destroyedWallPrefab;
    // Wall that will stay after destruction
    [SerializeField] private GameObject serverPrefab;

    // bool to prevent multiple destruction calls
    private bool isDestroyed = false;
    // Reference to the NetworkObject component
    private NetworkObject netObj;

    private void Awake()
    {
        netObj = GetComponent<NetworkObject>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Only process collisions on the server and if the wall isn't already destroyed
        if (!IsServer || isDestroyed) return;

        // Calculate the impact strength 
        float impactStrength = collision.relativeVelocity.magnitude;
        requiredHealth -= impactStrength;

        // Check if the impact is strong enough to destroy the wall or if the wall's health has been depleted
        if (impactStrength >= requiredImpact || requiredHealth <= 0f)
        {
            DestroyWall();
        }
    }

    private void DestroyWall()
    {
        // Prevent multiple destruction calls
        if (isDestroyed) return;
        isDestroyed = true;

        // Spawn fragments locally on the server
        Instantiate(destroyedWallPrefab, transform.position, transform.rotation);

        // Tell all clients to spawn fragments
        if (IsServer)
        {
            SpawnFragmentsClientRpc(transform.position, transform.rotation);
        }

        // Spawn one object that is on server
        if (IsServer && serverPrefab != null)
        {
            GameObject serverObj = Instantiate(serverPrefab, transform.position, transform.rotation);
            NetworkObject serverNetObj = serverObj.GetComponent<NetworkObject>();

            // only spawn if you set the variable in the inspector to prevent errors
            if (serverNetObj != null)
            {
                serverNetObj.Spawn(true);
            }
        }

        // Despawn the intact wall across the network
        if (netObj != null && netObj.IsSpawned)
        {
            // destroys on server and all clients
            netObj.Despawn(true); 
        }
        else
        {
            // fallback for non-networked objects
            Destroy(gameObject);
        }
    }

    // ClientRpc to spawn fragments on all clients
    [ClientRpc]
    private void SpawnFragmentsClientRpc(Vector3 position, Quaternion rotation)
    {
        // server already spawned
        if (IsServer) return;
        // Spawn fragments on clients
        Instantiate(destroyedWallPrefab, position, rotation);
    }
}