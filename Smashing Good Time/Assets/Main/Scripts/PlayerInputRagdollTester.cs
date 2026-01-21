using UnityEngine;

[RequireComponent(typeof(RagdollController))]
public class PlayerRagdollInput : MonoBehaviour
{
    public RagdollController ragdoll;
    public ThirdPersonOrbitCamera orbitCam;
    public Camera mainCam;
    public Transform cameraDefaultParent; // where the first-person camera sits
    public float launchForward = 4f;
    public float launchUp = 2f;

    void Awake()
    {
        if (ragdoll == null) ragdoll = GetComponent<RagdollController>();
        if (mainCam == null) mainCam = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            // simple test launch vector relative to forward
            Vector3 launch = transform.forward * launchForward + Vector3.up * launchUp;
            ragdoll.ActivateRagdoll(launch, ForceMode.VelocityChange);

            // switch camera to orbit around pelvis
            if (orbitCam != null)
            {
                orbitCam.SetTarget(ragdoll.pelvis);
                orbitCam.EnableOrbit(true);
            }

            // detach/disable FPS camera so it doesn't fight the orbit camera
            if (mainCam != null && cameraDefaultParent != null)
                mainCam.transform.SetParent(null, true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (ragdoll.IsRagdolled())
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ragdoll.TryGetUp();

                // reattach camera to player after a short delay (you may want to delay until stand animation completes)
                if (mainCam != null && cameraDefaultParent != null)
                {
                    mainCam.transform.SetParent(cameraDefaultParent, true);
                    mainCam.transform.localPosition = Vector3.zero;
                    mainCam.transform.localRotation = Quaternion.identity;
                }

                if (orbitCam != null)
                    orbitCam.EnableOrbit(false);

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}


// using UnityEngine;

// public class PlayerInputRagdollTester : MonoBehaviour
// {
//     public RagdollController ragdoll;

//     void Awake()
//     {

//     }

//     void Update()
//     {
//         // if (Input.GetKeyDown(KeyCode.R))
//         // {
//         //     ragdoll.RagdollModeOn();
//         // }

//         // if (ragdoll.IsRagdolled())
//         // {
//         //     if (Input.GetKeyDown(KeyCode.Space))
//         //     {
//         //         Debug.Log("Getting up from ragdoll");
//         //         ragdoll.RagdollModeOff();
//         //     }
//         // }
//     }
// }