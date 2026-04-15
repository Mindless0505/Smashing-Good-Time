using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Camera")]
    public Camera playerCamera;
    public float sprintFOV = 75f;
    public float normalFOV = 60f;
    public float fovSpeed = 8f;

    [Header("Movement")]
    public float moveSpeed;
    public float originalMoveSpeed;
    public float sprintSpeed;
    bool isSprinting;
    public float crouchSpeed;
    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public bool jumpReady;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    
    public float playerHeight;
    public LayerMask WhatIsGround;
    public bool grounded;

    public RagdollController Ragdoll;

    public Transform orientation;

    public float horizontalInput;
    public float verticalInput;

    private ChatManager ChatManager;
    public Animator animator;
    int isWalkingHash;
    int isSprintingHash;

    Vector3 moveDirection;

    Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        isWalkingHash = Animator.StringToHash("isWalking");
        isSprintingHash = Animator.StringToHash("isSprinting");
    }

    // Update is called once per frame
    void Update()
    {

        if (!IsOwner)
        {
            return;
        }

        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, WhatIsGround);

        if (!Ragdoll.RagMode)
        {
        MyInput();
        SpeedControl();
        HandleFOV();
        }

        // handle drag
        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;

        if (ChatManager.Instance != null && ChatManager.chatOpen) return;
    }
    private void FixedUpdate() 
    {
        MovePlayer();
    }

    private void MyInput() 
    {
        // Default speed
        moveSpeed = originalMoveSpeed;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        bool isWalking = animator.GetBool(isWalkingHash);
        bool isSprintingAnim = animator.GetBool(isSprintingHash);
        bool walkPress = Input.GetKey("w") || Input.GetKey("s");
        if (!isWalking && walkPress)
        {
            animator.SetBool("isWalking", true);
        }

        if (isWalking && !walkPress)
        {
            animator.SetBool("isWalking", false);
        }

        if (walkPress && isSprinting && grounded && !isSprintingAnim)
        {
            animator.SetBool("isSprinting", true);
        }

        if (!isSprinting && isSprintingAnim)
        {
            animator.SetBool("isSprinting", false);
        }
        

        // jump
        if (Input.GetKey(jumpKey) && jumpReady && grounded) 
        {
            jumpReady = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
        

        // Sprint
        if (Input.GetKey(sprintKey))  // remove && grounded
        {
            isSprinting = true;
            if (grounded) Sprint(); // still only apply sprint speed on ground
        }
        else
        {
            isSprinting = false;
            // if (isSprintingAnim) animator.SetBool("isSprinting", false);
        }

        // Crouch (overrides sprint if both pressed)
        if (Input.GetKey(crouchKey))
        {
            Crouch();
        }

    }

    private void MovePlayer() 
    {

   
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        // on ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        // in air
        else
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void Jump() 
    {
        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void Sprint()
    {
        moveSpeed = sprintSpeed;
        
    }

    private void Crouch()
    {
        moveSpeed = crouchSpeed;
    }
    private void ResetJump() 
    {
        jumpReady = true;
    }

    private void SpeedControl() 
    {         
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed) 
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void HandleFOV()
{
    float targetFOV = isSprinting ? sprintFOV : normalFOV;

    playerCamera.fieldOfView = Mathf.Lerp(
        playerCamera.fieldOfView,
        targetFOV,
        Time.deltaTime * fovSpeed
    );
}

}
