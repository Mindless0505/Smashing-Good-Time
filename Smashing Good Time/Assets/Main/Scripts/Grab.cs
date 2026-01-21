using UnityEngine;

public class Grab : MonoBehaviour
{

    public Camera cam;
    public float grabDistance = 2f;     // how far you can grab things
    public float holdDistance = 3f;     // where the object is held
    public float grabForce = 800f;      // how strong the pull is
    public float dropForceLimit = 30f;  // max force before dropping
    // public float maxGrabMass = 15f;

    public float throwForce= 1f;

    public float adjustedGrabForce;
    public float adjustedDropLimit;
    


    public LayerMask grabLayer;

    public Rigidbody heldObject;
    float distanceToObject;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

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

    public void TryGrab()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, grabDistance, grabLayer))
        {
            if (hit.rigidbody != null)
            {
                // if (GetComponent<Rigidbody>().mass > maxGrabMass)
                // {
                //     return;
                // }

                heldObject = hit.rigidbody;
                heldObject.useGravity = false;
                heldObject.linearDamping = 1f; // smoother control
                distanceToObject = hit.distance;

                float mass = heldObject.mass;
                adjustedGrabForce = grabForce / Mathf.Clamp(mass/2, 1f, 20f); 
                adjustedDropLimit = dropForceLimit / Mathf.Clamp(mass * 0.5f, 1f, 20f);
            }
        }
    }

    void FixedUpdate()
    {
        if (heldObject != null)
        {
            Vector3 holdPoint = cam.transform.position + cam.transform.forward * holdDistance;
            Vector3 toHold = holdPoint - heldObject.position;

            // Apply force toward hold position
            heldObject.AddForce(toHold * adjustedGrabForce * Time.fixedDeltaTime, ForceMode.Acceleration);


        // Quaternion targetRot = Quaternion.LookRotation(cam.transform.forward);
        // heldObject.MoveRotation(Quaternion.Slerp(heldObject.rotation, targetRot, Time.fixedDeltaTime * 5f));

            // Drop if object fights too hard
            if (heldObject.linearVelocity.magnitude > adjustedDropLimit || toHold.magnitude > grabDistance * 1.15f)
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
        if(heldObject == null) return;

        heldObject.useGravity = true;
        heldObject.linearDamping = 0f;
        heldObject = null;
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
        heldObject.useGravity = true;
        heldObject.AddForce(cam.transform.forward * throwForce, ForceMode.Force);
        DropObject();
    }

}

