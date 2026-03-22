using UnityEngine;
using Unity.Netcode;

public class SwapOwnership : NetworkBehaviour
{
    private Rigidbody rb;

    void Awake() => rb = GetComponent<Rigidbody>();

    void OnCollisionEnter(Collision col)
    {
        if (!IsServer) return;
        NetworkObject player = col.gameObject.GetComponent<NetworkObject>();
        if (player == null) return;
        NetworkObject.ChangeOwnership(player.OwnerClientId);
    }
}
