using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;


public class ObjectGrabbable : NetworkBehaviour
{

    private float throwForce = 25f;



    private Rigidbody objectRigidbody;
    private Transform objectGrabPointTransform;
    private SharedPhysics heldSharedPhysics;

    private void Awake()
    {
        objectRigidbody = GetComponent<Rigidbody>();
        
    }

    public override void OnNetworkSpawn()
    {
        heldSharedPhysics = GetComponent<SharedPhysics>();
    }

    public void Grab(Transform objectGrabPointTransform)
    {
        this.objectGrabPointTransform = objectGrabPointTransform;
        SetGravity(false, 5f, 5f);
        // objectRigidbody.useGravity = false;
        // objectRigidbody.linearDamping = 5f;
        // objectRigidbody.angularDamping = 5f;
    }

    public void Drop(){
        this.objectGrabPointTransform = null;
        SetGravity(true, 0f, .05f);
        // objectRigidbody.useGravity = true;
        // objectRigidbody.linearDamping = 0f;
        // objectRigidbody.angularDamping = .05f;
        
    }

    public void Throw(Vector3 throwDirection){
        this.objectGrabPointTransform = null;
        SetGravity(true, 0f, .05f);
        // objectRigidbody.useGravity = true;
        // objectRigidbody.linearDamping = 0f;
        // objectRigidbody.angularDamping = .05f;
        // heldSharedPhysics.ApplyForceServerRpc(throwDirection * throwForce, ForceMode.Impulse);
        objectRigidbody.AddForce(throwDirection * throwForce, ForceMode.Impulse);
    }

    private void Update()
    {
        if (objectGrabPointTransform != null)
        {
            float lerpSpeed = 30f;
            float t = 1 - Mathf.Pow(1 - 0.95f, Time.deltaTime * lerpSpeed);
            Vector3 newPosition = Vector3.Lerp(transform.position, objectGrabPointTransform.position, t);
            objectRigidbody.MovePosition(newPosition);
            // MoveServerRpc(newPosition);
        }
    }

    // [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    // private void MoveServerRpc(Vector3 newPosition)
    // {
    //     objectRigidbody.MovePosition(newPosition);
    // }


    // [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void SetGravity(bool useGravity, float drag, float angDrag)
    {
        objectRigidbody.useGravity = useGravity;
        objectRigidbody.linearDamping = drag;
        objectRigidbody.angularDamping = angDrag;
    }

}
