using Unity.Netcode;
using UnityEngine;

public class LocalPhysicsPush : NetworkBehaviour
{
    void OnCollisionEnter(Collision col)
    {
        if (!IsOwner) return;
        if (col.transform.root.CompareTag("Player")) return;

        SharedPhysics sp = col.gameObject.GetComponent<SharedPhysics>();
        if (sp == null) return;

        Vector3 force = -col.impulse * 1f;
        sp.ApplyForceServerRpc(force, ForceMode.Impulse);
    }
}