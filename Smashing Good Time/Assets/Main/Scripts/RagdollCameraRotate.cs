using System;
using UnityEngine;
using Unity.Netcode;

public class RagdollCameraRotate : NetworkBehaviour
{
    public float sensX = 200f;
    public float sensY = 200f;

    public Transform orientation;
    public Transform target; // ragdoll (usually hips)

    public float distance = 5f;

    public float xRotation;
    public float yRotation;

    void Start()
    {
        if(!IsOwner) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if(!IsOwner) return;
        if(target == null) return;

        // mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Math.Clamp(xRotation, -80f, 80f);

        
    }
    
    void LateUpdate()
    {
        if(!IsOwner) return;
        if(target == null) return;
        UpdateCamera();
    }


    void UpdateCamera()
    {
        // rotation around ragdoll
        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0);

        // position camera behind target
        Vector3 offset = rotation * new Vector3(0, 0, -distance);

        transform.position = target.position + offset;
        transform.LookAt(target);

        // orientation used for movement direction
        if(orientation != null)
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}