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


 

    void Awake()
    {   
        GatherRagdollBones();
        RagdollOff();
    }



    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (animator.enabled)
            {
                RagdollOn();
            }
            else
            {
                RagdollOff();
            }
        }   
    }

    void RagdollOn()
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

}