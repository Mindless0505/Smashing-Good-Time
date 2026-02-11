using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    public Animator animator;
    public GameObject PlayerRig;
    public Transform pelvis;
    public Collider Hitbox; 
    public Rigidbody MainRigidbody;
    public bool RagMode = false;

    [SerializeField] private float requiredImpact = 5f;

    private Vector3 SavedVelocity;
    private Vector3 SavedAngularVelocity;

    public float FlyMultiplier = 100f;



 

    void Awake()
    {   
        GatherRagdollBones();
        RagdollOff();
    }

    void OnCollisionEnter(Collision collision)
    {
        float impactStrength = collision.relativeVelocity.magnitude;
        if (impactStrength >= requiredImpact)
        {

            CaptureVelocity();
            RagdollOn();
        }
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (animator.enabled)
            {
                // CaptureVelocity();
                // RagdollOn();
            }
            else
            {
                RagdollOff();
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
            SetVelocity();
    }

        void RagdollOff()
    {
            RagMode = false;
            
            HitboxBringItBack();

            foreach(Collider col in ragdollColliders)
            {
                col.enabled = false;
            }

            foreach (Rigidbody rigid in limbsRigidbodies)
            {
                rigid.isKinematic = true;
            }

            MainRigidbody.isKinematic = false;
            Hitbox.enabled = true;
            animator.enabled = true;

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

            MainRigidbody.isKinematic = true;
            foreach (Rigidbody limb in limbsRigidbodies)
            {
                limb.AddForce(forceDir*FlyMultiplier, ForceMode.Impulse);
            }
            
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

    public void CaptureVelocity()
    {
        SavedVelocity = MainRigidbody.linearVelocity;
        SavedAngularVelocity = MainRigidbody.angularVelocity;
    }

    void SetVelocity()
    {
        foreach (Rigidbody rb in limbsRigidbodies)
        {
            rb.linearVelocity = SavedVelocity;
            rb.angularVelocity = SavedAngularVelocity;
        }
    }

    public void RecieveHit(Vector3 forceDir)
    {
        if (!RagMode)
        {
            CaptureVelocity();
            SpecialRagdollOn(forceDir);


        }
    }

}