using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

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

    [SerializeField] private float requiredImpact = 15f;

    private Vector3 SavedVelocity;
    private Vector3 SavedAngularVelocity;

    public float FlyMultiplier = 100f;

    [SerializeField] private Camera MainCam;
    [SerializeField] private AudioListener MainCamAudio;
    [SerializeField] private Camera RagCam;
    [SerializeField] private AudioListener RagCamAudio;

    public SledgeAttack Sledge;
 

    void Awake()
    {   
        GatherRagdollBones();
        RagdollOff(MainTransform.position);

        RagCam.enabled=false;
        RagCamAudio.enabled=false;


    }

    void OnCollisionEnter(Collision collision)
    {
        float impactStrength = collision.relativeVelocity.magnitude;
        if (impactStrength >= requiredImpact)
        {

            CaptureVelocityMain();
            RagdollOn();
        }
    }

    void Update()
    {

        if(!IsOwner)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!animator.enabled)
            {
                TryStand();
            }
        }   
    }

    public void RagdollOn()
    {
            
            RagMode = true;
            animator.enabled = false;
            Hitbox.enabled = false;
            
            foreach(Collider col in ragdollColliders)
            {
                col.enabled = true;
            }

            foreach (Rigidbody rigid in limbsRigidbodies)
            {
                rigid.isKinematic = false;
            }

            MainRigidbody.isKinematic = true;
            SetVelocityToRag();

            MainCam.enabled = false;
            MainCamAudio.enabled = false;
            RagCam.enabled=true;
            RagCamAudio.enabled=true;
            Sledge.SetVisualsActive(false);


    }

        void RagdollOff(Vector3 standPos)
    {
            CaptureVelocityRag();

            foreach(Collider col in ragdollColliders)
            {
                col.enabled = false;
            }

            foreach (Rigidbody rigid in limbsRigidbodies)
            {
                rigid.isKinematic = true;
            }

            RagMode = false;

            animator.applyRootMotion = false;
            MainTransform.position = standPos;
            MainRigidbody.isKinematic = false;
            Hitbox.enabled = true;
            animator.enabled = true;

            SetVelocityToMain();

            MainCam.enabled = true;
            MainCamAudio.enabled = true;
            RagCam.enabled=false;
            RagCamAudio.enabled=false;
            Sledge.SetVisualsActive(true);
    }

    

        public void SpecialRagdollOn(Vector3 forceDir)
    {
            
            RagMode = true;
            animator.enabled = false;
            Hitbox.enabled = false;
            
            foreach(Collider col in ragdollColliders)
            {
                col.enabled = true;
            }

            foreach (Rigidbody rigid in limbsRigidbodies)
            {
                rigid.isKinematic = false;
            }
            SetVelocityToRag();
            MainRigidbody.isKinematic = true;
            foreach (Rigidbody limb in limbsRigidbodies)
            {
                limb.AddForce(forceDir*FlyMultiplier, ForceMode.Impulse);
            }
            
            MainCam.enabled = false;
            MainCamAudio.enabled = false;
            RagCam.enabled=true;
            RagCamAudio.enabled=true;
            Sledge.SetVisualsActive(false);

            
    }


       Collider[] ragdollColliders;
       Rigidbody[] limbsRigidbodies;

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

    public void RecieveHit(Vector3 forceDir)
    {
        if (!RagMode)
        {
            CaptureVelocityMain();
            SpecialRagdollOn(forceDir);


        }
    }

    public void TryStand()
    {
        Vector3 pelvisPos = pelvis.position;

        Vector3 bottom = pelvis.position + Vector3.up * standheight;
        Vector3 top = bottom + Vector3.up * checkheight; 
        float radius = 0.4f; 

        if (!Physics.CheckCapsule(bottom, top, radius, collisionMask))
        {
            Vector3 standPos = bottom + Vector3.up * standheight;
            RagdollOff(standPos);
            return;
        }

        
        float searchRadius = searchStep; 
        while (searchRadius <= 20f) //change float to increase search radius. obviously.
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
                    Vector3 standPos = bottomCandidate + Vector3.up * standheight;
                    RagdollOff(standPos);
                    return;
                }
            }

            searchRadius += searchStep;
        }
    }

}




    

