namespace EasyPeasyFirstPersonController
{
    using UnityEngine;

    public partial class FirstPersonController1 : MonoBehaviour
    {
        [Range(0, 100)] public float mouseSensitivity = 50f;
        [Range(0f, 200f)] private float snappiness = 100f;

        [Range(0f, 20f)] public float walkSpeed = 3f;
        [Range(0f, 30f)] public float sprintSpeed = 5f;
        [Range(0f, 10f)] public float crouchSpeed = 1.5f;

        public float crouchHeight = 1f;
        public float crouchCameraHeight = 1f;

        public float slideSpeed = 8f;
        public float slideDuration = 0.7f;
        public float slideFovBoost = 5f;
        public float slideTiltAngle = 5f;

        [Range(0f, 15f)] public float jumpForce = 5f;

        public bool coyoteTimeEnabled = true;
        [Range(0.01f, 0.3f)] public float coyoteTimeDuration = 0.2f;

        public float normalFov = 60f;
        public float sprintFov = 70f;
        public float fovChangeSpeed = 5f;

        public float walkingBobbingSpeed = 10f;
        public float bobbingAmount = 0.05f;

        private float sprintBobMultiplier = 1.5f;
        private float recoilReturnSpeed = 8f;

        public bool canSlide = true;
        public bool canJump = true;
        public bool canSprint = true;
        public bool canCrouch = true;

        public QueryTriggerInteraction ceilingCheckQueryTriggerInteraction = QueryTriggerInteraction.Ignore;
        public QueryTriggerInteraction groundCheckQueryTriggerInteraction = QueryTriggerInteraction.Ignore;

        public Transform groundCheck;
        public float groundDistance = 0.2f;
        public LayerMask groundMask;

        public Transform playerCamera;
        public Transform cameraParent;

        private Rigidbody rb;
        private CapsuleCollider capsule;
        private Camera cam;

        private float rotX, rotY;
        private float xVelocity, yVelocity;

        private bool isGrounded;
        private Vector2 moveInput;

        public bool isSprinting;
        public bool isCrouching;
        public bool isSliding;

        private float slideTimer;
        private Vector3 slideDirection;

        private float originalHeight;
        private float originalCameraParentHeight;

        private float coyoteTimer;
        private float bobTimer;

        private float currentCameraHeight;
        private float currentBobOffset;

        private float currentFov;
        private float fovVelocity;

        private float currentSlideSpeed;
        private float slideSpeedVelocity;

        private float currentTiltAngle;
        private float tiltVelocity;

        private Vector3 recoil = Vector3.zero;

        private bool isLook = true;
        private bool isMove = true;

        public float CurrentCameraHeight =>
            isCrouching || isSliding ? crouchCameraHeight : originalCameraParentHeight;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            capsule = GetComponent<CapsuleCollider>();

            rb.useGravity = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            cam = playerCamera.GetComponent<Camera>();

            originalHeight = capsule.height;
            originalCameraParentHeight = cameraParent.localPosition.y;

            currentCameraHeight = originalCameraParentHeight;
            currentFov = normalFov;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            rotX = transform.rotation.eulerAngles.y;
            rotY = playerCamera.localRotation.eulerAngles.x;
            xVelocity = rotX;
            yVelocity = rotY;
        }

        private void Update()
        {
            GroundCheck();
            HandleLook();
            HandleState();
            HandleHeadBob();
            HandleFov();
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }

        private void GroundCheck()
        {
            isGrounded = Physics.CheckSphere(
                groundCheck.position,
                groundDistance,
                groundMask,
                groundCheckQueryTriggerInteraction
            );

            if (isGrounded)
                coyoteTimer = coyoteTimeEnabled ? coyoteTimeDuration : 0f;
            else if (coyoteTimeEnabled)
                coyoteTimer -= Time.deltaTime;
        }

        private void HandleLook()
        {
            if (!isLook) return;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime * 10f;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime * 10f;

            rotX += mouseX;
            rotY -= mouseY;
            rotY = Mathf.Clamp(rotY, -90f, 90f);

            xVelocity = Mathf.Lerp(xVelocity, rotX, snappiness * Time.deltaTime);
            yVelocity = Mathf.Lerp(yVelocity, rotY, snappiness * Time.deltaTime);

            float targetTilt = isSliding ? slideTiltAngle : 0f;
            currentTiltAngle = Mathf.SmoothDamp(currentTiltAngle, targetTilt, ref tiltVelocity, 0.2f);

            playerCamera.localRotation = Quaternion.Euler(yVelocity - currentTiltAngle, 0f, 0f);
            transform.rotation = Quaternion.Euler(0f, xVelocity, 0f);
        }

        private void HandleState()
        {
            moveInput.x = Input.GetAxis("Horizontal");
            moveInput.y = Input.GetAxis("Vertical");

            isSprinting = canSprint && Input.GetKey(KeyCode.LeftShift) &&
                          moveInput.y > 0.1f && !isCrouching && !isSliding;

            bool wantsCrouch = canCrouch && Input.GetKey(KeyCode.LeftControl) && !isSliding;
            isCrouching = wantsCrouch;

            if (canSlide && isSprinting && Input.GetKeyDown(KeyCode.LeftControl) && isGrounded)
            {
                isSliding = true;
                slideTimer = slideDuration;
                slideDirection = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;
                currentSlideSpeed = sprintSpeed;
            }

            if (isSliding)
            {
                slideTimer -= Time.deltaTime;
                if (slideTimer <= 0f || !isGrounded)
                    isSliding = false;

                float targetSpeed = slideSpeed * Mathf.Lerp(0.7f, 1f, slideTimer / slideDuration);
                currentSlideSpeed = Mathf.SmoothDamp(
                    currentSlideSpeed, targetSpeed, ref slideSpeedVelocity, 0.2f);
            }

            float targetHeight = isCrouching || isSliding ? crouchHeight : originalHeight;
            capsule.height = Mathf.Lerp(capsule.height, targetHeight, Time.deltaTime * 10f);
            capsule.center = new Vector3(0f, capsule.height * 0.5f, 0f);
        }

        private void HandleMovement()
        {
            if (!isMove) return;

            float speed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);
            Vector3 moveDir = transform.right * moveInput.x + transform.forward * moveInput.y;
            Vector3 horizontalVelocity = moveDir.normalized * speed;

            if (isSliding)
                horizontalVelocity = slideDirection * currentSlideSpeed;

            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);

            if (canJump && Input.GetKeyDown(KeyCode.Space) && (isGrounded || coyoteTimer > 0f))
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }

        private void HandleHeadBob()
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            bool moving = flatVel.magnitude > 0.1f;

            float targetBob = moving ? Mathf.Sin(bobTimer) * bobbingAmount : 0f;
            currentBobOffset = Mathf.Lerp(currentBobOffset, targetBob, Time.deltaTime * walkingBobbingSpeed);

            float targetCamHeight = isCrouching || isSliding ? crouchCameraHeight : originalCameraParentHeight;
            currentCameraHeight = Mathf.Lerp(currentCameraHeight, targetCamHeight, Time.deltaTime * 10f);

            cameraParent.localPosition = new Vector3(
                cameraParent.localPosition.x,
                currentCameraHeight + currentBobOffset,
                cameraParent.localPosition.z
            );

            if (moving && isGrounded)
            {
                float bobSpeed = walkingBobbingSpeed * (isSprinting ? sprintBobMultiplier : 1f);
                bobTimer += Time.deltaTime * bobSpeed;
                recoil.z = moveInput.x * -2f;
            }
            else
            {
                bobTimer = 0f;
                recoil = Vector3.zero;
            }

            cameraParent.localRotation =
                Quaternion.RotateTowards(cameraParent.localRotation, Quaternion.Euler(recoil),
                    recoilReturnSpeed * Time.deltaTime);
        }

        private void HandleFov()
        {
            float targetFov = isSprinting ? sprintFov :
                isSliding ? sprintFov + slideFovBoost : normalFov;

            currentFov = Mathf.SmoothDamp(currentFov, targetFov, ref fovVelocity, 1f / fovChangeSpeed);
            cam.fieldOfView = currentFov;
        }

        public void SetControl(bool state)
        {
            isLook = state;
            isMove = state;
        }

        public void SetCursorVisibility(bool visible)
        {
            Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = visible;
        }
    }
}
