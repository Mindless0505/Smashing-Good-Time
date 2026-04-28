using Unity.Netcode;
using UnityEngine;

public class SharedPhysics : NetworkBehaviour
{
    public Rigidbody rb;
    void Awake() => rb = GetComponent<Rigidbody>();

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ApplyForceServerRpc(Vector3 force, ForceMode mode)
    {
        rb.AddForce(force, mode);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ApplyForceAtPositionServerRpc(Vector3 force, Vector3 position, ForceMode mode)
    {
        rb.AddForceAtPosition(force, position, mode);
    }


    
}