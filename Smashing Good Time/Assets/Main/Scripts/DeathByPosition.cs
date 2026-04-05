using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class DeathByPosition : NetworkBehaviour
{
    [Header("Wall Limits")]
    public float minY = -10f;
    public float maxY = 100f;
    public float maxX = 100f;
    public float minX = -100f;
    public float maxZ = 100f;
    public float minZ = -100f;

    [Header("Respawn Settings")]
    public Transform respawnPoint;
    public float respawnDelay = 1f;

    [Header("Lives")]
    public int lives = 3;

    public NetworkVariable<int> currentLives = new NetworkVariable<int>(3,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private bool isRespawning = false;
    private Rigidbody rb;
    public PlayerHealth Health;

    public override void OnNetworkSpawn()
    {
        currentLives.OnValueChanged += OnLivesChanged;

        if (IsServer)
            currentLives.Value = lives;

        if (respawnPoint == null)
            respawnPoint = GameObject.Find("RespawnPoint").transform;
    }

    private void OnLivesChanged(int oldVal, int newVal)
    {
        // Tell HUD to update this player's lives display
        if (HUDManager.Instance != null)
            HUDManager.Instance.UpdateLives(OwnerClientId, newVal, lives);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Health = GetComponent<PlayerHealth>();

    }

    void Update()
    {
        if (!IsServer || !IsSpawned || isRespawning) return;

        Vector3 pos = transform.position;

        if (pos.y >= maxY || pos.y <= minY || pos.x >= maxX || pos.x <= minX || pos.z >= maxZ || pos.z <= minZ)
        {
            Die();
        }
    }

    void Die()
    {
        isRespawning = true;
        currentLives.Value--;
        Health.ResetHealth();

        if (currentLives.Value <= 0)
        {
            // Handle game over logic here (e.g., show game over screen, reset level, etc.)
            GameOver();
            return;
        }
        Respawn();
    }

    void Respawn()
    {
        TeleportOwnerRpc(respawnPoint.position);
        StartCoroutine(RespawnDelay()); // Delay re-enabling Die() so NetworkTransform has time to sync the new position
    }

    [Rpc(SendTo.Owner)]
    private void TeleportOwnerRpc(Vector3 position)
    {
        GetComponent<NetworkTransform>().Teleport(position, transform.rotation, transform.localScale);
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
        }
    }

    private IEnumerator RespawnDelay()
    {
        yield return new WaitForSeconds(0.5f);
        isRespawning = false;
    }

    void GameOver()
    {
        Debug.Log("Game over");
        gameObject.SetActive(false);

    }
}
