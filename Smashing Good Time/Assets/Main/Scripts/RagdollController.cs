using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RagdollController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Transform pelvis;
    public CharacterController characterController;    // ADDED
    public Collider armColliderToDisable;              // ADDED — the arm collider that normally surrounds CC
    public MonoBehaviour movementComponent;

    [Header("Ragdoll Settings")]
    public float getUpDelay = 0.6f;
    public float getUpBlend = 0.25f;
    public float pelvisSnapDistance = 0.2f;

    [Header("CharacterController Shrink Settings")]
    public float shrunkenHeight = 0.2f;
    public float shrunkenRadius = 0.15f;
    public Vector3 ccHiddenOffset = new Vector3(0.1f, 0.2f, 0.05f); // offset inside the arm

    private float originalHeight;
    private float originalRadius;
    private Vector3 originalCenter;

    private List<Rigidbody> ragdollBodies = new List<Rigidbody>();
    private List<Collider> ragdollColliders = new List<Collider>();
    private bool isRagdolled = false;
    private bool canGetUp = false;

    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();

        originalHeight = characterController.height;
        originalRadius = characterController.radius;
        originalCenter = characterController.center;

        GatherRagdollBones();
        SetRagdollState(false, true);
    }

    void Update()
    {
        // Make the CC follow the pelvis WHILE ragdolled
        if (isRagdolled)
        {
            Vector3 p = pelvis.position + ccHiddenOffset;
            characterController.transform.position = p;
        }
    }

    // =====================================================================
    // GATHER RAGDOLL BONES
    // =====================================================================
    void GatherRagdollBones()
    {
        ragdollBodies.Clear();
        ragdollColliders.Clear();

        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>(true);

        foreach (var rb in rbs)
        {
            if (rb.gameObject == this.gameObject) continue;

            ragdollBodies.Add(rb);

            var col = rb.GetComponent<Collider>();
            if (col != null) ragdollColliders.Add(col);
        }
    }

    // =====================================================================
    // ENABLE/DISABLE RAGDOLL
    // =====================================================================
    void SetRagdollState(bool enabled, bool forceKinematicStart = false)
    {
        foreach (var rb in ragdollBodies)
        {
            rb.isKinematic = !enabled;
            if (forceKinematicStart)
                rb.isKinematic = true;
        }

        foreach (var col in ragdollColliders)
            col.enabled = enabled;

        if (animator != null)
            animator.enabled = !enabled;

        // DO NOT DISABLE THE CC ANYMORE
        // Instead shrink/hide it
        if (enabled)
        {
            HideCharacterControllerInsideArm();
            if (armColliderToDisable != null) 
                armColliderToDisable.enabled = false; // avoid CC collision
        }
        else
        {
            RestoreCharacterController();
            if (armColliderToDisable != null)
                armColliderToDisable.enabled = true;
        }

        if (movementComponent != null)
            movementComponent.enabled = !enabled;

        isRagdolled = enabled;
    }

    // =====================================================================
    // ACTIVATE RAGDOLL WITH IMPULSE
    // =====================================================================
    public void ActivateRagdoll(Vector3 initialImpulse, ForceMode mode = ForceMode.Impulse)
    {
        if (isRagdolled) return;

        SetRagdollState(true);

        if (pelvis != null)
        {
            var pelvisRb = pelvis.GetComponent<Rigidbody>();
            if (pelvisRb != null)
                pelvisRb.AddForce(initialImpulse, mode);
        }

        canGetUp = false;
        StopAllCoroutines();
        StartCoroutine(EnableGetUpAfterDelay());
    }

    IEnumerator EnableGetUpAfterDelay()
    {
        yield return new WaitForSeconds(getUpDelay);
        canGetUp = true;
    }

    // =====================================================================
    // APPLY FORCE TO RAGDOLL
    // =====================================================================
    public void ApplyForceToRagdoll(Vector3 force, Vector3 point, ForceMode mode = ForceMode.Impulse)
    {
        if (!isRagdolled) return;

        Rigidbody nearest = null;
        float bestDist = float.MaxValue;

        foreach (var rb in ragdollBodies)
        {
            float d = Vector3.Distance(rb.worldCenterOfMass, point);
            if (d < bestDist)
            {
                bestDist = d;
                nearest = rb;
            }
        }

        if (nearest != null)
            nearest.AddForceAtPosition(force, point, mode);
    }

    // =====================================================================
    // STAND UP
    // =====================================================================
    public void TryGetUp()
    {
        if (!isRagdolled || !canGetUp) return;
        StartCoroutine(StandUpRoutine());
    }

    IEnumerator StandUpRoutine()
    {
        Vector3 pelvisPos = pelvis.position;

        // Freeze ragdoll
        foreach (var rb in ragdollBodies)
            rb.isKinematic = true;

        // Snap root to pelvis
        Vector3 target = pelvisPos;
        target.y += 0.2f;  // small lift
        transform.position = target;

        if (animator != null)
            animator.enabled = true;

        yield return new WaitForSeconds(getUpBlend);

        // CC reset
        RestoreCharacterController();

        // Re-enable arm collider
        if (armColliderToDisable != null)
            armColliderToDisable.enabled = true;

        // Disable ragdoll colliders
        foreach (var col in ragdollColliders)
            col.enabled = false;

        isRagdolled = false;
        canGetUp = false;
    }
}

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using EasyPeasyFirstPersonController;

// public class RagdollController : MonoBehaviour
// {
//     // public CharacterController mainCollider;
//     public Animator animator;
//     public GameObject ThisGuysRig;
//     public Transform pelvis;
//     private bool isRagdolled = false;
//     // public FirstPersonController MovementController;
//     // public SledgeAttack Sledge;

//     void Start()
//     {
//         GatherRagdollBones();

//     }


//     void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.R))
//         {

        
//             RagdollModeon();
//         }

//         if (isRagdolled && Input.GetKeyDown(KeyCode.Space))
//         {

//             RagdollModeoff();
//         }
//     }

//     Collider[] ragDollColliders;
//     Rigidbody[] ragDollRigidbodies;
//     void GatherRagdollBones()
//     {
//         ragDollColliders = ThisGuysRig.GetComponentsInChildren<Collider>();
//         ragDollRigidbodies = ThisGuysRig.GetComponentsInChildren<Rigidbody>();
//     }


//     public bool IsRagdolled()
//     {
//         return isRagdolled;
//     }

//     public void RagdollModeon()
//     {
//         animator.enabled = false;
//         isRagdolled = true;
   

//         // Sledge.SetVisualsActive(Sledge.sledgeHammer, false);
//     }

//     public void RagdollModeoff()
//     {
//         animator.enabled = true;
//         isRagdolled = false;
//     }

//     // void CopyPose(Transform source, Transform destination)
//     // {
//     //     foreach (Transform child in source)
//     //     {
//     //         Transform destChild = destination.Find(child.name);
//     //         if (destChild != null)
//     //         {
//     //             destChild.position = child.position;
//     //             destChild.rotation = child.rotation;
//     //             CopyPose(child, destChild);
//     //         }
//     //     }
//     // }
   
// }



// // problems
// //sledge needs to toggle with ragdoll\
// // grab needs to toggle too
// // camera needs to not follow player when ragdolled
// //fix visuals
// // make getting up work
// // getting hit makes you ragdoll
// // add force to ragdoll and keep velocity






// // [RequireComponent(typeof(Animator))]
// // public class RagdollController : MonoBehaviour
// // {
// //     [Header("References")]
// //     public Animator animator;                 // assign (optional - will auto-get)
// //     public Transform pelvis;                  // drag Hips/Pelvis bone here
// //     public Collider mainCollider;             // e.g. CapsuleCollider of CharacterController
// //     public MonoBehaviour movementComponent;   // your player movement script (disable while ragdoll)

// //     [Header("Ragdoll Settings")]
// //     public float getUpDelay = 0.6f;           // minimum time ragdoll lasts before standing
// //     public float getUpBlend = 0.25f;          // blend time when re-enabling animator
// //     public float pelvisSnapDistance = 0.2f;   // how close root will snap to pelvis on getup

// //     private List<Rigidbody> ragdollBodies = new List<Rigidbody>();
// //     private List<Collider> ragdollColliders = new List<Collider>();
// //     private bool isRagdolled = false;
// //     private bool canGetUp = false;

// //     void Awake()
// //     {
// //         if (animator == null) animator = GetComponentInChildren<Animator>();
// //         GatherRagdollBones();
// //         SetRagdollState(false, true);
// //     }

// //     void GatherRagdollBones()
// //     {
// //         ragdollBodies.Clear();
// //         ragdollColliders.Clear();
// //         Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>(true);
// //         foreach (var rb in rbs)
// //         {
// //             // ignore root rigidbody if present on player root
// //             if (rb.gameObject == this.gameObject) continue;
// //             ragdollBodies.Add(rb);
// //             var col = rb.GetComponent<Collider>();
// //             if (col != null) ragdollColliders.Add(col);
// //         }
// //     }

// //     // sets ragdoll on/off. "forceKinematicStart" used at init to avoid toggling animator repeatedly
// //     void SetRagdollState(bool enabled, bool forceKinematicStart = false)
// //     {
// //         foreach (var rb in ragdollBodies)
// //         {
// //             rb.isKinematic = !enabled;
// //             if (forceKinematicStart)
// //             {
// //                 rb.isKinematic = true;
// //             }
// //         }

// //         foreach (var col in ragdollColliders)
// //             col.enabled = enabled;

// //         if (animator != null)
// //             animator.enabled = !enabled;

// //         if (mainCollider != null) mainCollider.enabled = !enabled;
// //         if (movementComponent != null) movementComponent.enabled = !enabled;

// //         isRagdolled = enabled;
// //     }

// //     public bool IsRagdolled() => isRagdolled;

// //     /// <summary>
// //     /// Turn ragdoll on and apply an optional initial launch/impulse to the pelvis.
// //     /// (Call this from hit/weapon code; over network call as RPC then call locally.)
// //     /// </summary>
// //     public void ActivateRagdoll(Vector3 initialImpulse, ForceMode mode = ForceMode.Impulse)
// //     {
// //         if (isRagdolled) return;

// //         // disable animator + movement, enable bones physics
// //         SetRagdollState(true);

// //         // apply impulse to pelvis rigidbody if set
// //         if (pelvis != null)
// //         {
// //             var pelvisRb = pelvis.GetComponent<Rigidbody>();
// //             if (pelvisRb != null)
// //             {
// //                 pelvisRb.AddForce(initialImpulse, mode);
// //             }
// //         }

// //         canGetUp = false;
// //         StopAllCoroutines();
// //         StartCoroutine(EnableGetUpAfterDelay());
// //     }

// //     IEnumerator EnableGetUpAfterDelay()
// //     {
// //         yield return new WaitForSeconds(getUpDelay);
// //         canGetUp = true;
// //     }

// //     /// <summary>
// //     /// Apply external force to ragdoll (e.g. a hammer hit). Will add force at contact point to create torque.
// //     /// </summary>
// //     public void ApplyForceToRagdoll(Vector3 force, Vector3 point, ForceMode mode = ForceMode.Impulse)
// //     {
// //         if (!isRagdolled) return;
// //         // find nearest rigidbody to contact point (simple)
// //         Rigidbody nearest = null;
// //         float bestDist = float.MaxValue;
// //         foreach (var rb in ragdollBodies)
// //         {
// //             float d = Vector3.Distance(rb.worldCenterOfMass, point);
// //             if (d < bestDist)
// //             {
// //                 bestDist = d; nearest = rb;
// //             }
// //         }
// //         if (nearest != null)
// //             nearest.AddForceAtPosition(force, point, mode);
// //     }

// //     /// <summary>
// //     /// Attempt to stand up (blends animator back on and snaps root to pelvis)
// //     /// </summary>
// //     public void TryGetUp()
// //     {
// //         if (!isRagdolled || !canGetUp) return;
// //         StartCoroutine(StandUpRoutine());
// //     }

// //     IEnumerator StandUpRoutine()
// //     {
// //         // capture pelvis world pose
// //         Vector3 pelvisPos = pelvis != null ? pelvis.position : transform.position;
// //         Quaternion pelvisRot = pelvis != null ? pelvis.rotation : transform.rotation;

// //         // disable ragdoll physics first (freeze poses)
// //         foreach (var rb in ragdollBodies) rb.isKinematic = true;

// //         // snap root position near pelvis so animator plays in correct place
// //         Vector3 rootTarget = pelvisPos;
// //         rootTarget.y = transform.position.y; // optionally keep same height to avoid sinking through ground
// //         transform.position = rootTarget;

// //         // enable animator and blend (you may trigger a get-up animation here)
// //         if (animator != null)
// //         {
// //             animator.enabled = true;
// //             // optional: set animator parameters to match body pose, or play a GetUp animation trigger here
// //         }

// //         // re-enable main collider & movement after a tiny delay to allow animator to place feet
// //         yield return new WaitForSeconds(getUpBlend);

// //         if (mainCollider != null) mainCollider.enabled = true;
// //         if (movementComponent != null) movementComponent.enabled = true;

// //         // finalize: ensure colliders disabled on bones and ragdoll flag cleared
// //         foreach (var col in ragdollColliders) col.enabled = false;
// //         isRagdolled = false;
// //         canGetUp = false;
// //     }
// // }