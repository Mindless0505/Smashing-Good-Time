using UnityEngine;
using Unity.Netcode;


public class RagCamScript : NetworkBehaviour
{
    [SerializeField] private Transform pelvis;
    [SerializeField] private Rigidbody ragdollRigidbody;

    [SerializeField] private Camera MainCam;
    [SerializeField] private Camera RagCam;

    
    [SerializeField] private float distanceBehind = 3f;
    [SerializeField] private float heightAbove = 0.3f;

    private Vector3 offset;
    private float lockedY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void LateUpdate()
    {

        if(!IsOwner)
        {
            return;
        }

        if(RagCam.enabled ==true)
        {
            RagCamOn();
        }

    }


    // public void InitializeCamera(Vector3 initialDirection)
    // {

    //     initialDirection.y = 0;
    //     initialDirection.Normalize();

    //     offset = -initialDirection * distance;
    //     offset.y = height;

    //     lockedY = pelvis.position.y + height;

    //     RagCam.transform.position = pelvis.position + offset;

    //     Vector3 lookTarget = pelvis.position;
    //     lookTarget.y = RagCam.transform.position.y;

    //     RagCam.transform.LookAt(lookTarget);
    // }


    void RagCamOn()
    {


        // {
        //     Vector3 lookDirection;

        //     Vector3 lookDirection = Vector3.zero;
        //     if (ragdollRigidbody != null && ragdollRigidbody.linearVelocity.sqrMagnitude > 0.001f)
        //     {
        //         lookDirection = ragdollRigidbody.linearVelocity.normalized;
        //     }
        //     else
        //     {
        //         lookDirection = pelvis.forward;
        //     }

        //     Vector3 targetPosition = pelvis.position - lookDirection * distanceBehind + Vector3.up * heightAbove;

        //     RagCam.transform.position = targetPosition;

        //     Vector3 lookTarget = pelvis.position;
        //     lookTarget.y = RagCam.transform.position.y;

        //     RagCam.transform.LookAt(lookTarget);
        // }
    }
}
