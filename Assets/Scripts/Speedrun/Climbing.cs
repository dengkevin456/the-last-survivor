using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climbing : MonoBehaviour
{
    [Header("References")] public Transform orientation;
    private PlayerMovement pm;
    public Rigidbody rb;
    public LayerMask whatIsGround;

    [Header("Climbing")] public float climbSpeed;
    public float maxClimbTime;
    private float climbTimer;

    private bool climbing;

    [Header("Climb Jumping")] public float climbJumpUpForce;
    public float climbJumpBackForce;

    public KeyCode jumpKey = KeyCode.Space;
    public int climbJumps;
    private int climbJumpsLeft;

    [Header("Detection")] public float detectionLength;
    public float sphereCastRadius;
    public float maxWallLookAngle;
    private float wallLookAngle;

    private Transform lastWall;
    private Vector3 lastWallNormal;
    public float minWallNormalAngleChange;

    [Header("Exiting stuff")] public bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    private RaycastHit frontWallHit;
    private bool wallFront;

    private void Start()
    {
        pm = GetComponent<PlayerMovement>();
    }

    private void WallCheck()
    {
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, 
            orientation.forward, out frontWallHit, detectionLength, whatIsGround);
        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);
        bool newWall = frontWallHit.transform != lastWall ||
                       Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallNormalAngleChange;
        
        
        if ((wallFront && newWall) || pm.isGrounded)
        {
            climbTimer = maxClimbTime;
            climbJumpsLeft = climbJumps;
        }
    }

    private void StartClimbing()
    {
        climbing = true;
        pm.climbing = true;
        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;
        // Camera fov change can go here
    }

    private void ClimbingMovement()
    {
        rb.velocity = new Vector3(rb.velocity.x, climbSpeed, rb.velocity.z);
        // SFX can go here
    }

    private void StopClimbing()
    {
        climbing = false;
        pm.climbing = false;
        // Particle effect can go here
    }

    private void StateMachine()
    {
        // State 1 - Climbing
        if (wallFront && Input.GetKey(KeyCode.W) && wallLookAngle < maxWallLookAngle)
        {
            if (!climbing && climbTimer > 0) StartClimbing();
            // Timer
            if (climbTimer > 0) climbTimer -= Time.deltaTime;
            if (climbTimer < 0) StopClimbing();
        }
        // State 2 - Exiting
        else if (exitingWall)
        {
            if (climbing) StopClimbing();
            if (exitWallTimer > 0) exitWallTimer -= Time.deltaTime;
            if (exitWallTimer < 0) exitingWall = false;
        }
        else
        {
            if (climbing) StopClimbing();
        }
        
        if (wallFront && Input.GetKeyDown(jumpKey) && climbJumpsLeft > 0) ClimbJump();
    }

    private void ClimbJump()
    {
        exitingWall = true;
        exitWallTimer = exitWallTime;
        Vector3 forceToApply = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumpBackForce;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }

    private void Update()
    {
        if (PauseMenu.gameIsPaused) return;
        WallCheck();
        StateMachine();
        
        if (climbing && !exitingWall) ClimbingMovement();
    }
}
