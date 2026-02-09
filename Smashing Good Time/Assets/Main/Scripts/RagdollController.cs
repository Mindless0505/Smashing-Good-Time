using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    public Animator animator;
    public Transform pelvis;
    public Collider Hitbox; 


    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();

        // GatherRagdollBones();

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
                Debug.Log("Ragdolloff");
                RagdollOff();
            }
        }   
    }

    void RagdollOn()
    {

            Hitbox.enabled = false;
            animator.enabled = false;

    }

        void RagdollOff()
    {

            Hitbox.enabled = true;
            animator.enabled = true;

    }


    // void GatherRagdollBones()
    // {

    //     Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>(true);

    //     foreach (var rb in rbs)
    //     {
    //         if (rb.gameObject == this.gameObject) continue;

    //         ragdollBodies.Add(rb);

    //         var col = rb.GetComponent<Collider>();
    //         if (col != null) ragdollColliders.Add(col);
    //     }
    // }
}