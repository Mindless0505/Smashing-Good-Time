using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class PlayerCamera : NetworkBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;

    public float xRotation;
    public float yRotation;

    public float sensitivityStep = 10f;
    public float minSensitivity = 10f;
    public float maxSensitivity = 500f;

    public Camera MainCam;
    public Transform MainTransform;
    public bool CamOn;
    void Start()
    {
        if(!IsOwner) return;
        // lock cursor in middle and stop it from being visible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false; // disable the whole script on non-owners
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // if(SceneManager.GetActiveScene().name =="Game")
        // {

        // }
        if(!IsOwner)
        {
            return;
        }

        //if (Input.GetKeyDown(KeyCode.Equals)) // + key (same key as = on most keyboards)
        //{
        //    sensX = Mathf.Clamp(sensX + sensitivityStep, minSensitivity, maxSensitivity);
        //    sensY = sensX;
        //    PlayerPrefs.SetFloat("Sensitivity", sensX);
        //}

        //if (Input.GetKeyDown(KeyCode.Minus))
        //{
        //    sensX = Mathf.Clamp(sensX - sensitivityStep, minSensitivity, maxSensitivity);
        //    sensY = sensX;
        //    PlayerPrefs.SetFloat("Sensitivity", sensX);
        //}

        // get the mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensX;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Math.Clamp(xRotation,-90f,90f);

        // rotate cam and orientation
        transform.rotation = Quaternion.Euler(xRotation,yRotation,0);
        orientation.rotation = Quaternion.Euler(0,yRotation,0);


        if (CamOn && IsOwner)
        {
            Vector3 camForward = MainCam.transform.forward;
            camForward.y = 0f;

        if (camForward.sqrMagnitude > 0.001f)
        {
            camForward.Normalize();
            MainTransform.forward = camForward;
        }
        }
    }
}
