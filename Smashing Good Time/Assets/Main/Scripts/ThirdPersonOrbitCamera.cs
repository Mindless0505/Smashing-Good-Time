using UnityEngine;

public class ThirdPersonOrbitCamera : MonoBehaviour
{
    [Header("Targeting")]
    public Transform target;               // set to pelvis when ragdolled
    public Vector3 pivotOffset = new Vector3(0, 1.2f, 0);

    [Header("Orbit Settings")]
    public float distance = 3.5f;
    public float minDistance = 1.5f;
    public float maxDistance = 6f;
    public float xSpeed = 180f;
    public float ySpeed = 120f;
    public float minY = -20f;
    public float maxY = 60f;
    public bool invertY = false;

    private float yaw = 0f;
    private float pitch = 10f;
    private bool enabledOrbit = false;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void LateUpdate()
    {
        if (!enabledOrbit || target == null) return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y") * (invertY ? 1 : -1);

        yaw += mouseX * xSpeed * Time.deltaTime;
        pitch += mouseY * ySpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minY, maxY);

        float scroll = -Input.mouseScrollDelta.y;
        distance = Mathf.Clamp(distance + scroll * 0.5f, minDistance, maxDistance);

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0);
        Vector3 pivot = target.position + pivotOffset;
        Vector3 pos = pivot + rot * new Vector3(0, 0, -distance);

        transform.position = pos;
        transform.rotation = rot;
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }

    public void EnableOrbit(bool on)
    {
        enabledOrbit = on;
        // optionally show/hide crosshair or HUD here
    }
}


// using UnityEngine;

// public class ThirdPersonOrbitCamera : MonoBehaviour
// {
//     [Header("Targeting")]
//     public Transform target;               // set to pelvis when ragdolled
//     public Vector3 pivotOffset = new Vector3(0, 1.2f, 0);

//     [Header("Orbit Settings")]
//     public float distance = 3.5f;
//     public float minDistance = 1.5f;
//     public float maxDistance = 6f;
//     public float xSpeed = 180f;
//     public float ySpeed = 120f;
//     public float minY = -20f;
//     public float maxY = 60f;
//     public bool invertY = false;

//     public Camera camera;

// // Please REemmber to unbind the character to the camera when ragdolled so he doesnt fucking move with it god fuckkk
//     private float yaw = 0f;
//     private float pitch = 10f;
//     private bool enabledOrbit = false;

//     void Start()
//     {
//         Vector3 angles = transform.eulerAngles;
//         yaw = angles.y;
//         pitch = angles.x;
//     }

//     void LateUpdate()
//     {
//         if (!enabledOrbit || target == null) return;

//         float mouseX = Input.GetAxis("Mouse X");
//         float mouseY = Input.GetAxis("Mouse Y") * (invertY ? 1 : -1);

//         yaw += mouseX * xSpeed * Time.deltaTime;
//         pitch += mouseY * ySpeed * Time.deltaTime;
//         pitch = Mathf.Clamp(pitch, minY, maxY);

//         float scroll = -Input.mouseScrollDelta.y;
//         distance = Mathf.Clamp(distance + scroll * 0.5f, minDistance, maxDistance);

//         Quaternion rot = Quaternion.Euler(pitch, yaw, 0);
//         Vector3 pivot = target.position + pivotOffset;
//         Vector3 pos = pivot + rot * new Vector3(0, 0, -distance);

//         transform.position = pos;
//         transform.rotation = rot;
//     }

//     public void SetTarget(Transform t)
//     {
//         target = t;
//     }

//     public void EnableOrbit(bool on)
//     {
//         ShowHead(on);
//         enabledOrbit = on;
//         // optionally show/hide crosshair or HUD here
//     }

//     public void ShowHead(bool enabled)
//     {
//         if (enabled)
//         {
//         camera.cullingMask |= (1 << 6);
//         }
//         else
//         {
//         camera.cullingMask &= ~(1 << 6);
//         }
//     }
// }