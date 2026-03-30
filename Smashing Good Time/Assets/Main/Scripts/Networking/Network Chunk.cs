using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using System.Collections;

public class NetworkChunk : NetworkBehaviour
{
    private NetworkRigidbody netRb;
    private NetworkTransform netTransform;
    private Rigidbody rb;
    public bool isHeld = false;

    [SerializeField] private float sleepDelay = 3f; // how long to wait before disabling sync
    private float stillTimer = 0f;
    private bool isNetworkActive = false;

    private void Awake()
    {
        netRb = GetComponent<NetworkRigidbody>();
        netTransform = GetComponent<NetworkTransform>();
        rb = GetComponent<Rigidbody>();

        if (netRb != null) netRb.enabled = false;
        if (netTransform != null) netTransform.enabled = false;
    }

private IEnumerator SleepCheck()
{
    while (true)
    {
        yield return new WaitForSeconds(0.5f);

        if (isHeld) continue; // don't sleep while being held

        if (rb.IsSleeping() || rb.linearVelocity.magnitude < 0.05f)
        {
            stillTimer += 0.5f;
            if (stillTimer >= sleepDelay)
            {
                DisableNetworking();
                yield break;
            }
        }
        else
        {
            stillTimer = 0f;
        }
    }
}

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer || isNetworkActive) return;
        EnableNetworking();
        StartCoroutine(SleepCheck());
    }


    private void EnableNetworking()
    {
        isNetworkActive = true;
        stillTimer = 0f;
        if (netRb != null) netRb.enabled = true;
        if (netTransform != null) netTransform.enabled = true;
    }

    private void DisableNetworking()
    {
        isNetworkActive = false;
        stillTimer = 0f;
        if (netRb != null) netRb.enabled = false;
        if (netTransform != null) netTransform.enabled = false;
    }

    public void OnGrabbed()
    {  
        isHeld = true;
        EnableNetworkingServerRpc();
    }

    public void OnDropped()
    {
        isHeld = false;
        // Small delay so the throw velocity has time to sync before disabling
        stillTimer = -2f;
    }

    [Rpc(SendTo.Server)]
    private void EnableNetworkingServerRpc()
    {
        netRb.enabled = true;
        StartCoroutine(SleepCheck());
    }

    [Rpc(SendTo.Server)]
    private void DisableNetworkingServerRpc()
    {
        netRb.enabled = false;
        netTransform.enabled = false;
    }
}