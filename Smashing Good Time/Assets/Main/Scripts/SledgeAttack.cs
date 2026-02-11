using UnityEngine;
using System;
using System.Collections;
using Random = UnityEngine.Random;

public class SledgeAttack : MonoBehaviour
{

    Animator animator;
    AudioSource audioSource;
    public Animator hammerAnimator;

    // public GameObject hammer;

    public float attackDistance = 3f;
    public float attackDelay = 0.4f;
    public float attackSpeed = 1f;
    public int attackDamage= 1;
    public float HammerForce =5f;
    public LayerMask attackLayer;

    public AudioClip hammerSwing;
    public AudioClip hitSound;

    bool attacking = false;
    bool readyToAttack = true;
    int attackCount;

    public Camera cam;

    public Grab grabscript;
    public GameObject sledgeHammer;

    public GameObject playerRoot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
    //   isGrounded = controller.isGrounded;
        if(grabscript.heldObject == null)
        {

            SetVisualsActive(sledgeHammer, true);
            if(Input.GetMouseButtonDown(0))
            {Attack();}
        }
        else
        {
            SetVisualsActive(sledgeHammer, false);
        }
        
      

    //   SetAnimations();  
    }

    public void SetVisualsActive(GameObject parent, bool active)
    {
    // Get all Renderer components in parent and children
        Renderer[] renderers = parent.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            r.enabled = active; // show or hide
        }
    }

    public void Attack()
    {
        if(!readyToAttack || attacking) return;
        
        readyToAttack = false;
        attacking = true;

        // hammer.GetComponent<Animator>().Play("Hammerswing");

        // if (animator != null)
        //     animator.SetTrigger("Hammerswing");

        hammerAnimator.SetTrigger("Swing");

            Invoke(nameof(ResetAttack), attackSpeed);
            Invoke(nameof(AttackRaycast), attackDelay);

            audioSource.pitch = Random.Range(0.9f,1.1f);
            audioSource.PlayOneShot(hammerSwing);
        // hammer.GetComponent<Animator>().Play("Default");
      
    }


    void ResetAttack()
    {
        attacking=false;
        readyToAttack=true;
    }   

    void AttackRaycast()
    {
        if(sledgeHammer.GetComponent<Renderer>().enabled == true)
        {
            
            if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, attackDistance, attackLayer))
            {

                if(hit.transform.root == playerRoot.transform)
                    return;

                HitTarget(hit);
            }
        }
    }

    void HitTarget(RaycastHit hit)
    {
        audioSource.pitch = Random.Range(0.9f,1.1f);
        audioSource.PlayOneShot(hitSound);

        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb != null)
        {
            Vector3 forceDir = hit.point - cam.transform.position;
            forceDir.Normalize();
            if (!rb.CompareTag("Player"))
            {
                rb.AddForce(forceDir * HammerForce, ForceMode.Impulse);
            }
            else
            {
                RagdollController ragdoll = hit.transform.root.GetComponent<RagdollController>();
                if (ragdoll != null)
                {
                    ragdoll.RecieveHit(forceDir);
                }
            }



        }

    }

}
