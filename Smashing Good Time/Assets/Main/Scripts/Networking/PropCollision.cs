using Unity.Netcode;
using UnityEngine;

public class PropCollision : NetworkBehaviour
{
    private Rigidbody rb;
    void Awake() => rb = GetComponent<Rigidbody>();

    void OnCollisionEnter(Collision col)
    {
        if (!IsServer) return;
        if (col.transform.root.CompareTag("Player")) return;

        SharedPhysics sp = col.gameObject.GetComponent<SharedPhysics>();
        if (sp == null) return;

        Vector3 force = -col.contacts[0].normal * rb.linearVelocity.magnitude * rb.mass;
        sp.rb.AddForce(force, ForceMode.Impulse);
    }

}