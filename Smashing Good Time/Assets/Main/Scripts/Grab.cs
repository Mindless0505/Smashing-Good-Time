using UnityEngine;
using Unity.Netcode;
using System.Collections;
using UnityEngine.UI;

public class Grab : NetworkBehaviour
{

    public Camera cam;
    public float grabDistance = 2f;     // how far you can grab things
    public float holdDistance = 3f;     // where the object is held
    public float grabForce = 800f;      // how strong the pull is
    public float dropForceLimit = 30f;  // max force before dropping
    public float throwForce= 1f;

    public float adjustedGrabForce;
    public float adjustedDropLimit;
    
    public SledgeAttack Sledge;
    public LayerMask grabLayer;

    public Rigidbody heldObject;
    float distanceToObject;
    private SharedPhysics heldSharedPhysics;
    private RagdollController Ragdoll;

    [SerializeField] private RawImage XHair;
    [SerializeField] private RawImage GrabXHair;

    // Update is called once per frame
    void Awake()
    {
        Ragdoll = GetComponent<RagdollController>();
        XHair.enabled = true;
        GrabXHair.enabled = false;
        StartCoroutine(CrosshairCheck());
    }
    
    void Update()
    {
        if(!IsOwner)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null)
                TryGrab();
            else
                DropObject();
        }

        if (heldObject != null && Input.GetMouseButtonDown(0))
        {
            ThrowObject();
        }

    }

    private IEnumerator CrosshairCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.05f); // checks 20 times per second, plenty for a crosshair

            if (!IsOwner) continue;
            
            bool canGrab = Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, grabDistance) 
                        && !Ragdoll.RagMode && heldObject == null && hit.collider.CompareTag("Throwable");

            XHair.enabled = !canGrab;
            GrabXHair.enabled = canGrab;
        }
    }

    public void TryGrab()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, grabDistance, grabLayer))
        {
            if (hit.rigidbody != null)
            {
                heldSharedPhysics = hit.rigidbody.GetComponent<SharedPhysics>();
                if (heldSharedPhysics == null) return; // only grab objects with SharedPhysics

                Sledge.SetVisualsActive(false);

                heldObject = hit.rigidbody;
                heldObject.useGravity = false;
                heldObject.linearDamping = 1f; // smoother control
                distanceToObject = hit.distance;

                NetworkChunk chunk = hit.rigidbody.GetComponent<NetworkChunk>();
                if (chunk != null) chunk.OnGrabbed();

                float mass = heldObject.mass;
                // adjustedGrabForce = grabForce / Mathf.Clamp(mass/2, 1f, 20f); 
                // adjustedDropLimit = dropForceLimit / Mathf.Clamp(mass * 0.5f, 1f, 20f);
            }
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        if (heldObject != null)
        {
            Vector3 holdPoint = cam.transform.position + cam.transform.forward * holdDistance;
            Vector3 toHold = holdPoint - heldObject.position;

            // Apply force toward hold position
            heldSharedPhysics.ApplyForceServerRpc(toHold * grabForce * Time.fixedDeltaTime, ForceMode.Acceleration);

            
            if (heldObject.linearVelocity.magnitude > dropForceLimit || toHold.magnitude > grabDistance * 1.15f)
            {
                DropObject();
            }

            if (IsStandingOn(heldObject))
            {
                DropObject();
            }
        }
    }

    public void DropObject()
    {
        if (heldObject == null) return;

        // Cache reference BEFORE nulling
        NetworkChunk chunk = heldObject.GetComponent<NetworkChunk>();
        Rigidbody dropped = heldObject;

        SetGravityServerRpc(heldSharedPhysics.GetComponent<NetworkObject>(), true, 0f);

        heldSharedPhysics = null;
        heldObject = null;
        Sledge.SetVisualsActive(true);

        // Now safe to call
        if (chunk != null) chunk.OnDropped();
    }

    bool IsStandingOn(Rigidbody obj)
    {
       if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f))
        {   
            // If what we're standing on has a Rigidbody and it's the same as what we're holding
            if (hit.rigidbody != null && hit.rigidbody == obj)
            {
                return true;
            }
        }
        return false; 
    }

    void ThrowObject()
    {
        heldSharedPhysics.ApplyForceServerRpc(cam.transform.forward * throwForce, ForceMode.Force);
        DropObject();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void SetGravityServerRpc(NetworkObjectReference netObjRef, bool useGravity, float drag)
    {
        if (!netObjRef.TryGet(out NetworkObject netObj)) return;
        Rigidbody rb = netObj.GetComponent<Rigidbody>();
        if (rb == null) return;
        rb.useGravity = useGravity;
        rb.linearDamping = drag;
    }

    

}

