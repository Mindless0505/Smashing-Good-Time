using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
// using System.Numerics;

public class DeathByPosition : NetworkBehaviour
{
    [Header("Ragdoll")]
    public Transform bodyTarget;

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

    private bool isGameOver = false;
    private RagdollController ragdollController;


    public override void OnNetworkSpawn()
    {
        currentLives.OnValueChanged += OnLivesChanged;

        if (IsServer)
            currentLives.Value = lives;

        // Force HUD update since OnValueChanged won't fire for the initial set
        if (HUDManager.Instance != null)
            HUDManager.Instance.UpdateLives(OwnerClientId, lives, lives);

        if (respawnPoint == null)
            respawnPoint = GameObject.Find("RespawnPoint").transform;

        // Reset dead player state
        isGameOver = false;
        isRespawning = false;

        // Re-enable all MonoBehaviours
        foreach (var mono in GetComponents<MonoBehaviour>())
            mono.enabled = true;

        // Re-enable rig and hitbox
        if (ragdollController != null)
        {
            ragdollController.PlayerRig.SetActive(true);
            ragdollController.Hitbox.enabled = true;
        }

        // Re-enable renderers
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = true;

        // Re-enable player camera (owner only)
        if (IsOwner)
        {
            Camera playerCam = GetComponentInChildren<Camera>();
            if (playerCam != null)
                playerCam.enabled = true;

            // Disable spectator if it was active
            SpectatorController spectator = FindFirstObjectByType<SpectatorController>(FindObjectsInactive.Include);
            if (spectator != null)
                spectator.gameObject.SetActive(false);
        }
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
        ragdollController = GetComponent<RagdollController>();

        // Auto-grab hips from RagdollCameraRotate if not manually assigned
        if (bodyTarget == null)
        {
            var ragdollCam = GetComponent<RagdollCameraRotate>();
            if (ragdollCam != null && ragdollCam.target != null)
                bodyTarget = ragdollCam.target;
        }
    }

    void Update()
    {
        if (!IsServer || !IsSpawned || isRespawning || isGameOver) return;

        // bool isRagdolled = ragdollController != null && ragdollController.RagMode;
        // Vector3 pos = (isRagdolled && bodyTarget != null) ? bodyTarget.position : transform.position;
        Vector3 pos = ragdollController.pelvis.position;

        if (pos.y >= maxY || pos.y <= minY ||
            pos.x >= maxX || pos.x <= minX ||
            pos.z >= maxZ || pos.z <= minZ)
        {
            Die();
        }
    }

    void Die()
    {
        if (isGameOver || isRespawning) return;

        isRespawning = true;
        currentLives.Value--;
        Health.ResetHealth();

        if (currentLives.Value <= 0)
        {
            GameOver();
            return;
        }

        Respawn();
    }

    void Respawn()
    {
        Vector3 spawnPos = GetRandomSpawnPoint();
        // ragdollController.TryStand();
        TeleportOwnerRpc(spawnPos);
        StartCoroutine(RespawnDelay());
    }

    private Vector3 GetRandomSpawnPoint()
    {
        if (SpawnManager.Instance != null && SpawnManager.Instance.spawnPoints.Count > 0)
        {
            int index = Random.Range(0, SpawnManager.Instance.spawnPoints.Count);
            return SpawnManager.Instance.spawnPoints[index].position;
        }

        // Fallback to origin if SpawnManager not found
        Debug.LogWarning("SpawnManager not found, respawning at origin");
        return Vector3.zero;
    }

    [Rpc(SendTo.Owner)]
    private void TeleportOwnerRpc(Vector3 position)
    {
        if (ragdollController != null && ragdollController.RagMode)
        {
            // Freeze all ragdoll bones using the controller's own array
            foreach (var ragdollRb in ragdollController.limbsRigidbodies)
            {
                ragdollRb.linearVelocity = Vector3.zero;
                ragdollRb.angularVelocity = Vector3.zero;
            }
            // ragdollController.pelvis.position = position;
            ragdollController.RagdollOffServerRpc(position);
            
  
        }
        else
        {
            //normal teleport
            GetComponent<NetworkTransform>().Teleport(position, transform.rotation, transform.localScale);
        }  
    }

    private IEnumerator ReleaseRagdollAfterTeleport(Vector3 position)
    {
        // Hold for a few fixed frames so physics registers the ground
        yield return new WaitForSeconds(.2f);

        // Re-enable physics on ragdoll bones using the controller's array
        foreach (var ragdollRb in ragdollController.limbsRigidbodies)
        {
            ragdollRb.isKinematic = false;
            ragdollRb.linearVelocity = Vector3.zero;
            ragdollRb.angularVelocity = Vector3.zero;
            ragdollRb.Sleep();
        }

        // Teleport root now that bones are settled
        GetComponent<NetworkTransform>().Teleport(position, transform.rotation, transform.localScale);
    }

    private IEnumerator RespawnDelay()
    {
        yield return new WaitForSeconds(1f);
        isRespawning = false;
    }

    void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log($"Client {OwnerClientId} is out of lives");

        DisablePlayerServerRpc();

        EnterSpectatorModeRpc();
    }

    // Runs on server � disables the object for all clients
    [Rpc(SendTo.Server)]
    private void DisablePlayerServerRpc()
    {
        // Disable all MonoBehaviours except DeathByPosition and NetworkBehaviours
        // so the network object stays alive but the player can't do anything
        foreach (var mono in GetComponents<MonoBehaviour>())
            if (mono is not DeathByPosition && mono is not NetworkBehaviour)
                mono.enabled = false;

        // Disable the ragdoll rig entirely so no bones are active
        if (ragdollController != null)
            ragdollController.PlayerRig.SetActive(false);

        // Disable hitbox
        if (ragdollController != null)
            ragdollController.Hitbox.enabled = false;

        // Disable all renderers for all clients
        DisableVisualsClientRpc();


    }

    [ClientRpc]
    private void DisableVisualsClientRpc()
    {
        // Disable all renderers on every client so no one can see the dead body
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        // Disable the rig on all clients too
        if (ragdollController != null)
            ragdollController.PlayerRig.SetActive(false);
    }


    [Rpc(SendTo.Owner)]
    private void EnterSpectatorModeRpc()
    {
        // Disable player camera
        Camera playerCam = GetComponentInChildren<Camera>();
        if (playerCam != null)
            playerCam.enabled = false;

        // Disable ragdoll camera too so pressing R does nothing
        if (ragdollController != null)
        {
            ragdollController.enabled = false; // blocks R key input entirely
        }

        // Disable all input/movement scripts
        foreach (var mono in GetComponents<MonoBehaviour>())
            if (mono is not DeathByPosition && mono is not NetworkBehaviour)
                mono.enabled = false;

        // Start spectating
        SpectatorController spectator = FindFirstObjectByType<SpectatorController>(FindObjectsInactive.Include);
        if (spectator != null)
            spectator.StartSpectating();
    }
}
