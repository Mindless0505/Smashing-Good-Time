using UnityEngine;
using Unity.Netcode;

public class RagdollMoveCamera : NetworkBehaviour
{
    public Transform target;        // Assign ragdoll body part (hips/head)
    public Vector3 offset = new Vector3(0f, 2f, -4f);
    public float smoothSpeed = 5f;
    public bool lookAtTarget = true;

    void LateUpdate()
    {
        if(!IsOwner) return;

        if (target == null) return;

        // Desired camera position
        Vector3 desiredPosition = target.position + offset;

        // Smooth follow
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // Optional look at ragdoll
        if (lookAtTarget)
        {
            transform.LookAt(target);
        }
    }
}