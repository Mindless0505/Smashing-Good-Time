using UnityEngine;
using Unity.Netcode;
using System.Linq;
using System;

public class SpectatorController : MonoBehaviour
{
    [Header("Camera Settings")]
    public float sensX = 200f;
    public float sensY = 200f;
    public float distance = 5f;

    private float xRotation;
    private float yRotation;

    private Transform[] targets = new Transform[0];
    private int currentIndex = 0;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    public void StartSpectating()
    {
        gameObject.SetActive(true);
        RefreshTargets();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    void Update()
    {
        RefreshTargets();
        if (targets.Length == 0) return;

        HandlePlayerSwitch();
        HandleMouseInput();
    }

    void LateUpdate()
    {
        if (targets.Length == 0) return;
        UpdateCamera();
    }

    private void HandlePlayerSwitch()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.RightArrow))
            currentIndex = (currentIndex + 1) % targets.Length;

        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.LeftArrow))
            currentIndex = (currentIndex - 1 + targets.Length) % targets.Length;
    }

    private void HandleMouseInput()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Math.Clamp(xRotation, -80f, 80f);
    }

    private void UpdateCamera()
    {
        Transform target = targets[currentIndex];

        // Same orbital logic as RagdollCameraRotate
        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);

        transform.position = target.position + offset;
        transform.LookAt(target.position);
    }

    private void RefreshTargets()
    {
        targets = FindObjectsByType<NetworkObject>(FindObjectsSortMode.None)
            .Where(no => no.CompareTag("Player")
                      && no.gameObject.activeSelf
                      && !no.IsOwner)
            .Select(no =>
            {
                // If they're ragdolled, follow their ragdoll target (hips)
                // instead of the player root
                var ragdollCam = no.GetComponent<RagdollCameraRotate>();
                if (ragdollCam != null && ragdollCam.target != null)
                    return ragdollCam.target;

                return no.transform; // fallback to root if not ragdolled
            })
            .ToArray();

        currentIndex = Mathf.Clamp(currentIndex, 0, Mathf.Max(0, targets.Length - 1));
    }
}