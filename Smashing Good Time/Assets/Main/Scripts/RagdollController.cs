using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class RagdollController : NetworkBehaviour
{
    public Animator animator;
    public GameObject PlayerRig;
    public Transform pelvis;
    public Collider Hitbox; 
    public Rigidbody MainRigidbody;
    public Rigidbody PelvisRigidbody;
    public bool RagMode = false;
    public Transform MainTransform;
    
    public float standheight = 0.5f;
    public float checkheight = 2.5f;
    public float checkRadius = 0.5f;
    public float searchStep = 0.25f;
    public float maxSearchRadius = 1f;
    public LayerMask collisionMask;

    private float initialRagdollSpeed;
    private bool canStand = false;
    private float minimumThreshold = 0.5f;
    private float threshPercent = 0.05f;
    private float threshold = 0f;

    [SerializeField] private float requiredImpact = 15f;

    private Vector3 SavedVelocity;
    private Vector3 SavedAngularVelocity;

    // public float FlyMultiplier = 100f;

    [SerializeField] private Camera MainCam;
    [SerializeField] private AudioListener MainCamAudio;
    [SerializeField] private Camera RagCam;
    [SerializeField] private AudioListener RagCamAudio;

    public SledgeAttack Sledge;
    public PlayerHealth Health;

    void Awake()
    {   
        GatherRagdollBones();
        
        RagCam.enabled=false;
        RagCamAudio.enabled=false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        RagdollSetup();
    }

    void OnCollisionEnter(Collision collision)
    {
        
        float impactStrength = collision.relativeVelocity.magnitude;
        if (impactStrength >= requiredImpact && collision.gameObject.CompareTag("Throwable"))
        {
            Vector3 blockDirection = collision.impulse.normalized;
            CaptureVelocityMain();
            RecieveHitServerRpc(blockDirection, false);
        }
    }

    void Update()
    {

        if(!IsOwner)
        {
            return;
        }

        if (RagMode && !canStand)
        {
            float currentSpeed = PelvisRigidbody.linearVelocity.magnitude;
        
            if (currentSpeed <= Mathf.Max(threshold, minimumThreshold))
            {
                canStand = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!animator.enabled && canStand)
            {
                TryStand();
            }
        }  

        if (Input.GetKeyDown(KeyCode.R) && animator.enabled)
        {
                RagdollOn();
        }  
    }




    private void RagdollSetup()//none networked setup
    {
        foreach(Collider col in ragdollColliders)
            col.enabled = false;

        foreach (Rigidbody rigid in limbsRigidbodies)
            rigid.isKinematic = true;

        RagMode = false;
        animator.applyRootMotion = false;
        MainRigidbody.isKinematic = false;
        Hitbox.enabled = true;
        animator.enabled = true;

        if (IsOwner)
        {
            MainCam.enabled = true;
            MainCamAudio.enabled = true;
            RagCam.enabled = false;
            RagCamAudio.enabled = false;
            Sledge.SetVisualsActive(true);
        }
    }


    public void RagdollOn()
    {
        RagdollOnServerRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RagdollOnServerRpc()
    {
        RagdollOnClientRpc();
    }

    [ClientRpc]
    private void RagdollOnClientRpc()
    {
        CaptureVelocityMain();
        RagMode = true;
        animator.enabled = false;
        Hitbox.enabled = false;

        foreach(Collider col in ragdollColliders)
            col.enabled = true;

        foreach (Rigidbody rigid in limbsRigidbodies)
            rigid.isKinematic = false;

        MainRigidbody.isKinematic = true;
        SetVelocityToRag();

        initialRagdollSpeed = PelvisRigidbody.linearVelocity.magnitude;
        threshold = initialRagdollSpeed * threshPercent;
        canStand = false;

        // Cameras only matter for the owner
        if (IsOwner)
        {
            MainCam.enabled = false;
            MainCamAudio.enabled = false;
            RagCam.enabled = true;
            RagCamAudio.enabled = true;
            Sledge.SetVisualsActive(false);
        }
    }


    // void RagdollOff(Vector3 standPos)
    // {
    //     RagdollOffServerRpc(standPos);
    // }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RagdollOffServerRpc(Vector3 standPos)
    {
        RagdollOffClientRpc(standPos);
    }

    [ClientRpc]
    public void RagdollOffClientRpc(Vector3 standPos)
    {
        CaptureVelocityRag();

        foreach(Collider col in ragdollColliders)
            col.enabled = false;

        foreach (Rigidbody rigid in limbsRigidbodies)
        {
            rigid.linearVelocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
            rigid.isKinematic = true;
        }

        RagMode = false;
        animator.applyRootMotion = false;

        if (IsOwner)
        {
            var nt = GetComponent<NetworkTransform>();
            if (nt != null) nt.Teleport(standPos, MainTransform.rotation, MainTransform.localScale);
            else MainTransform.position = standPos;
        }

        MainTransform.position = standPos;
        MainRigidbody.isKinematic = false;
        Hitbox.enabled = true;
        animator.enabled = true;
        // SetVelocityToMain();


        if (IsOwner)
        {
            MainCam.enabled = true;
            MainCamAudio.enabled = true;
            RagCam.enabled = false;
            RagCamAudio.enabled = false;
            Sledge.SetVisualsActive(true);
            StartCoroutine(ForcePosition(standPos));
        }
}

    IEnumerator ForcePosition(Vector3 standPos)
    {
        // Hold position for just a few frames to beat the NetworkTransform sync
        for (int i = 0; i < 3; i++)
        {
            MainTransform.position = standPos;
            yield return null;
        }
        // Release with ragdoll momentum
        MainRigidbody.linearVelocity = SavedVelocity;
    }

    // public void SpecialRagdollOn(Vector3 forceDir, int multDiv)
    // {
    //     if (IsOwner)
    //         SpecialRagdollOnClientRpc(forceDir, multDiv);
    // }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RecieveHitServerRpc(Vector3 forceDir, bool Hammer)
    {
        int mult = Health.Hit(Hammer); 
        SpecialRagdollOnClientRpc(forceDir, mult, Hammer);
        
    }

    [ClientRpc]
    private void SpecialRagdollOnClientRpc(Vector3 forceDir,int mult, bool Hammer)
    {
        RagMode = true;
        animator.enabled = false;
        Hitbox.enabled = false;

        
        foreach(Collider col in ragdollColliders)
            col.enabled = true;

        foreach (Rigidbody rigid in limbsRigidbodies)
            rigid.isKinematic = false;

        SetVelocityToRag();
        MainRigidbody.isKinematic = true;
    
        if (Hammer)
        {
            foreach (Rigidbody limb in limbsRigidbodies)
                limb.AddForce(forceDir * mult, ForceMode.Impulse);
        }
        else
        {
            foreach (Rigidbody limb in limbsRigidbodies)
                limb.AddForce(forceDir * mult/2, ForceMode.Impulse);
        }
        
        initialRagdollSpeed = PelvisRigidbody.linearVelocity.magnitude;
        threshold = initialRagdollSpeed * threshPercent;
        canStand = false;

        if (IsOwner)
        {
            MainCam.enabled = false;
            MainCamAudio.enabled = false;
            RagCam.enabled = true;
            RagCamAudio.enabled = true;
            Sledge.SetVisualsActive(false);
        }
    }

    
    public void RecieveHit(Vector3 forceDir, bool Hammer)
    {
        if (!RagMode)
        {
            CaptureVelocityMain();
            RecieveHitServerRpc(forceDir, Hammer);
        }
    }






    Collider[] ragdollColliders;
    public Rigidbody[] limbsRigidbodies;

    void GatherRagdollBones()
    {
        ragdollColliders = PlayerRig.GetComponentsInChildren<Collider>();
        limbsRigidbodies = PlayerRig.GetComponentsInChildren<Rigidbody>();
    }

    public void HitboxBringItBack()
    {
        Hitbox.transform.position = pelvis.position;

        Vector3 euler = Hitbox.transform.eulerAngles;
        Hitbox.transform.rotation = Quaternion.Euler(0f, euler.y, 0f);
    }




    public void CaptureVelocityMain()//capture velocity of main body
    {
        SavedVelocity = MainRigidbody.linearVelocity;
        SavedAngularVelocity = MainRigidbody.angularVelocity;
    }
    public void CaptureVelocityRag()//capture velocity when ragdolled
    {
        SavedVelocity = PelvisRigidbody.linearVelocity;
        SavedAngularVelocity = PelvisRigidbody.angularVelocity;
    }

    void SetVelocityToRag()//set velocity of ragdoll
    {
        foreach (Rigidbody rb in limbsRigidbodies)
        {
            rb.linearVelocity = SavedVelocity;
            rb.angularVelocity = SavedAngularVelocity;
        }
    }
    void SetVelocityToMain()//sets velocity of main body
    {
        MainRigidbody.AddForce(SavedVelocity, ForceMode.VelocityChange);
    }




    public void TryStand()
    {
        TryStandServerRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void TryStandServerRpc()
    {
        Vector3 pelvisPos = pelvis.position;
        Vector3 bottom = pelvis.position + Vector3.up * standheight;
        Vector3 top = bottom + Vector3.up * checkheight;
        float radius = 0.4f;

        Vector3 standPos = Vector3.zero;
        bool found = false;

        if (!Physics.CheckCapsule(bottom, top, radius, collisionMask))
        {
            standPos = bottom + Vector3.up * standheight;
            found = true;
        }
        else
        {
            float searchRadius = searchStep;
            while (searchRadius <= 20f)
            {
                int points = 16;
                for (int i = 0; i < points; i++)
                {
                    float angle = i * Mathf.PI * 2f / points;
                    Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * searchRadius;
                    Vector3 candidatePos = pelvisPos + offset;
                    Vector3 bottomCandidate = candidatePos + Vector3.up * standheight;
                    Vector3 topCandidate = bottomCandidate + Vector3.up * checkheight;

                    if (!Physics.CheckCapsule(bottomCandidate, topCandidate, radius, collisionMask))
                    {
                        standPos = bottomCandidate + Vector3.up * standheight;
                        found = true;
                        break;
                    }
                }
                if (found) break;
                searchRadius += searchStep;
            }
        }

        if (!found) return;

        MainRigidbody.isKinematic = false;
        RagdollOffClientRpc(standPos);
    }

}




    

