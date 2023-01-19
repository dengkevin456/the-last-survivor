using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")] public Climbing climbingScript;
    public Transform cameraHolder;
    public Transform orientation;
    public Transform cameraPos;
    private Rigidbody rb;
    public Transform groundCheck;
    public LayerMask groundMask;
    public LayerMask enemyMask;
    public Transform gunTip;
    private Wallrunning wallRunningScript;
    private Camera playerCam;

    [Header("Crouching")] public float crouchSpeed;
    public float crouchYScale;
    public float startYScale;

    [Header("Slope handling")] public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;
    public float speedIncreaseMultiplier = 2f;
    public float slopeIncreaseMultiplier = 1.5f;

    [Header("Keybinds")] public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.Q;
    public KeyCode crouchKey = KeyCode.LeftShift;
    [Header("Jumping")] public float jumpForce;
    public float jumpCoolDown;
    public float airMultiplier;
    [Header("Settings")] public float mouseSensitivity;
    private Vector3 velocityToSet;
    public float moveSpeed = 5f;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public bool freeze;
    private bool enableMovementOnNextTouch;
    [Space]
    private float desiredMoveSpeed;
    [Space]
    private float lastDesiredMoveSpeed;
    public float groundDrag = 20f;
    private float xRotation;
    private float yRotation;
    private Vector3 moveDirection;
    private float horizontalInput, verticalInput;
    [HideInInspector]
    public bool isGrounded;
    private bool readyToJump;
    public MovementState state;
    [HideInInspector] public Vector3 nearestCheckpointPosition;
    [Header("Wallrun stuff")] public float wallrunSpeed = 10f;
    [Header("Climbing")] public float climbSpeed = 4f;
    public enum MovementState
    {
        freeze,
        walking,
        sprinting,
        crouching,
        climbing,
        sliding,
        wallrunning,
        air,
    }

    public bool sliding;
    public bool wallRunning;
    public bool climbing;
    public bool activeGrapple;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        wallRunningScript = GetComponent<Wallrunning>();
        playerCam = cameraHolder.GetComponentInChildren<Camera>();
    }

    private void Start()
    {
        readyToJump = true;
        Cursor.lockState = CursorLockMode.Locked;
        rb.freezeRotation = true;
        startYScale = transform.localScale.y;
        nearestCheckpointPosition = rb.position;
        
    }

    private void CameraMovement()
    { 
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;
        yRotation += mouseX;
        cameraHolder.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.transform.rotation = Quaternion.Euler(0, yRotation, 0);
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        if (Input.GetKey(jumpKey) && readyToJump && isGrounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCoolDown);
        }
        // STart crouch
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        // Stop crouch
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void MovePlayer()
    {
        if (activeGrapple) return;
        if (climbingScript.exitingWall) return;
        moveDirection = orientation.right * horizontalInput + orientation.forward * verticalInput;
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * (moveSpeed * 20f), ForceMode.Force);
            rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        else if (isGrounded)
            rb.AddForce(moveDirection.normalized * (moveSpeed * 10f), ForceMode.Force);
        else if (!isGrounded)
            rb.AddForce(moveDirection.normalized * (moveSpeed * 10f * airMultiplier), ForceMode.Force);

        if (!wallRunning)
        {
            rb.useGravity = !OnSlope();
        }
    }

    private void SpeedControl()
    {
        if (activeGrapple) return;
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }   
        }
    }

    public void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Jump()
    {
        exitingSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
                                               + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));
        return velocityXZ + velocityY;
    }

    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;
        velocityToSet = CalculateJumpVelocity(rb.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), .1f);
        Invoke(nameof(ResetRestrictions), 3f);
    }


    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.velocity = velocityToSet;
    }
    private void StateHandler()
    {
        // Mode: Freeze
        if (freeze)
        {
            state = MovementState.freeze;
            // desiredMoveSpeed = 0;
            rb.velocity = Vector3.zero;
        }
        // Mode: Climbing
        else if (climbing)
        {
            state = MovementState.climbing;
            desiredMoveSpeed = climbSpeed;
        }
        // Mode : wallrunning
        else if (wallRunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }
        // Mode: Sliding
        else if (sliding)
        {
            state = MovementState.sliding;
            if (OnSlope() && rb.velocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;
            else
                desiredMoveSpeed = sprintSpeed;
        }
        // Mode : crouching
        else if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        // Mode: Sprinting
        else if (isGrounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        // Mode: Walking
        else if (isGrounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        // Mode: In air
        else
        {
            state = MovementState.air;
        }
        

        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            // Keeping the deceleration instead of instantly decrease to 0
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            // StopAllCoroutines();
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;

    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            time += Time.deltaTime;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    private void CameraPosition()
    {
        cameraHolder.position = cameraPos.position;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit,
            1 + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    public void CheckpointRespawn()
    {
        rb.position = nearestCheckpointPosition;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void DoFov(float endValue)
    {
        cameraHolder.GetComponentInChildren<Camera>().DOFieldOfView(endValue, 0.25f);
    }

    public void DoTilt(float zTilt)
    {
        playerCam.transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }


    public void ResetRestrictions()
    {
        activeGrapple = false;
    }
    private void OnCollisionEnter(Collision other)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();
            GetComponentInChildren<Hooking>().StopGrapple();
        }
    }

    private void FixedUpdate()
    {
        if (PauseMenu.gameIsPaused || PauseMenu.levelComplete) return;
        MovePlayer();
    }

    private void Update()
    {
        if (PauseMenu.gameIsPaused || PauseMenu.levelComplete) return;
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.1f, groundMask);
        MyInput();
        StateHandler();
        SpeedControl();
        CameraMovement();
        CameraPosition(); 
        rb.drag = isGrounded && !activeGrapple && !ExplosiveBarrel.setSlowmotion ? groundDrag : 0f;               
    }
}
