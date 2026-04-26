using UnityEngine;
using System;
using System.Collections;
using Random = UnityEngine.Random;
using Unity.Netcode;
using Unity.VisualScripting;

public class SledgeAttack : NetworkBehaviour
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

    public GameObject sledgeHammer;

    public GameObject playerRoot;

    private bool VisualActive;
    [SerializeField] private Renderer hiltRenderer;

    
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsOwner)
        {
            return;
        }

        if(VisualActive==true)
        {

            if(Input.GetMouseButtonDown(0))
            {
                Attack();
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        int colorIndex = (int)OwnerClientId % ColorReference.PlayerColorsHilt.Length;
        
        // Instance the material so we don't change it for everyone
        Material hiltMat = hiltRenderer.material;
        hiltMat.color = ColorReference.PlayerColorsHilt[colorIndex];
    }

    public void SetVisualsActive(bool active)
    {
        if (IsSpawned)
        {
            SetVisualsActiveServerRpc(active);
        }
        else
        {
            ApplyVisuals(active);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetVisualsActiveServerRpc(bool active)
    {
        SetVisualsActiveClientRpc(active);
    }

    [ClientRpc]
    private void SetVisualsActiveClientRpc(bool active)
    {
        ApplyVisuals(active);
    }

    private void ApplyVisuals(bool active)
    {
        VisualActive = active;
        Renderer[] renderers = sledgeHammer.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = active;
        }
    }

    public void Attack()
    {
        if(!readyToAttack || attacking) return;
        
        readyToAttack = false;
        attacking = true;

        hammerAnimator.SetTrigger("Swing");

            Invoke(nameof(ResetAttack), attackSpeed);
            Invoke(nameof(AttackRaycast), attackDelay);

            PlaySwingSoundServerRpc();

      
    }


    void ResetAttack()
    {
        attacking=false;
        readyToAttack=true;
    }   

    void AttackRaycast()
    {
        if(!VisualActive) return;

        int rayCount = 8; // number of rays in the sweep
        float spreadAngle = 20f;

        for (int i = 0; i < rayCount; i++)
        {
             float angle = -spreadAngle / 2f + (spreadAngle / (rayCount - 1)) * i;
             Quaternion rotation = Quaternion.AngleAxis(angle, cam.transform.up);
             Vector3 dir = rotation * cam.transform.forward;

            if(Physics.Raycast(cam.transform.position, dir, out RaycastHit hit, attackDistance, attackLayer))
            {
                if (IsOwner && hit.transform.root == playerRoot.transform) continue;
                HitTarget(hit);
                break;
            }
        }

    }

    void HitTarget(RaycastHit hit)
    {
        PlayHitSoundServerRpc(hit.point);

        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb != null)
        {
            Vector3 forceDir = hit.point - cam.transform.position;
            forceDir.Normalize();

            if (!rb.CompareTag("Player"))
            {
 

                SharedPhysics sp = rb.GetComponent<SharedPhysics>();
                if (sp != null)
                {
                    NetworkObject netObj = sp.GetComponent<NetworkObject>();
                    if (netObj != null && netObj.IsSpawned)
                        {sp.ApplyForceServerRpc(forceDir * HammerForce, ForceMode.Impulse);}
                }
                else
                {
                    rb.AddForce(forceDir * HammerForce, ForceMode.Impulse); // just in case not networked ;D
                }
            }
            else
            {
                RagdollController ragdoll = hit.transform.root.GetComponent<RagdollController>();
                if (ragdoll != null)
                {
                    ragdoll.RecieveHit(forceDir , true);
                }
            }



        }

    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
private void PlaySwingSoundServerRpc()
{
    PlaySwingSoundClientRpc();
}

[ClientRpc]
private void PlaySwingSoundClientRpc()
{
    AudioUtils.PlaySoundFollowing(hammerSwing,transform, 0.9f, 1.1f, IsOwner);
}

[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
private void PlayHitSoundServerRpc(Vector3 hitpoint)
{
    PlayHitSoundClientRpc(hitpoint);
}

[ClientRpc]
private void PlayHitSoundClientRpc(Vector3 hitpoint)
{
    AudioUtils.PlayAtPoint(hitSound, hitpoint, .9f, 1.1f, IsOwner);
}

}
