using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BeatGameConfig : MonoBehaviour
{
    [Header("Speed")]
    private Rigidbody rb;
    public float moveSpeed;
    public float mouseSensitivity;
    [Header("References")]
    public Transform playerCam;
    public Transform orientation;
    public Transform cameraPos;
    public Transform groundCheck;
    private bool isGrounded;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private float xRotation, yRotation;
    private bool beatGame = false;
    private void Awake()
    {
        // Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponentInChildren<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void CameraLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;
        yRotation += mouseX;
        xRotation = Mathf.Clamp(xRotation, -88f, 88f);
        playerCam.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0f);
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");   
    }

    private void MovePlayer()
    {
        moveDirection = orientation.right * horizontalInput + orientation.forward * verticalInput;
        rb.AddForce(moveDirection.normalized * (moveSpeed * 10f), ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }   
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void Update()
    {
        playerCam.position = cameraPos.position;
        CameraLook();
        SpeedControl();
        MyInput();
    }
}
