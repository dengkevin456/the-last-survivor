using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class PlaygroundPlayer : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;

    private PhotonView PV;
    private float xRotation, yRotation;
    public Transform orientation;
    public Transform cameraPos;
    public GameObject cameraHolder;
    public Camera playerCam;
    public TextMeshPro usernameText;
    public Transform groundCheck;
    public LayerMask groundMask;
    [Header("UI stuff")] public Slider fovSlider;
    public GameObject pauseMenuUI;
    public TextMeshProUGUI fovValue;
    public Slider sensitivitySlider;
    public TextMeshProUGUI sensitivityValue;
    [Header("Keybinds")] public KeyCode jumpKey = KeyCode.Space;
    [Header("Player settings/inputs")]
    public float mouseSensitivity = 400f;
    public float moveSpeed = 7f;
    public float jumpForce = 7f;
    public float jumpCoolDown = .25f;
    public float airMultiplier = .4f;
    private bool readyToJump;
    private float horizontalInput, verticalInput;
    private Vector3 moveDirection;
    private bool isGrounded;
    [Header("Player local properties")] private Hashtable playerProperties = new Hashtable();
    // FOV stuff
    private float NORMAL_FOV = 70f;
    public float CAMERA_FOV = 70f;
    private float MIN_FOV = 30f, MAX_FOV = 120f;
    // Mosue sensitivity
    private float NORMAL_SENSITIVITY = 400f;
    private float MIN_SENSITIVITY = 100f, MAX_SENSITIVITY = 800f;
    private bool gameIsPaused;
    [Header("Gravity stuff")] private Vector3 normalGravity = new Vector3(.0f, -9.81f, .0f);
    private void Awake()
    {
        Physics.gravity = normalGravity;
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        readyToJump = true;
        if (!PV.IsMine)
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
        }
        
        CAMERA_FOV = NORMAL_FOV;
        mouseSensitivity = NORMAL_SENSITIVITY;
        pauseMenuUI.SetActive(false);
        playerProperties["gameIsPaused"] = gameIsPaused;
        playerProperties["cameraFOV"] = CAMERA_FOV;
        playerProperties["mouseSensitivity"] = NORMAL_SENSITIVITY / 4;
        fovSlider.value = CAMERA_FOV;
        playerCam.fieldOfView = CAMERA_FOV;
        fovSlider.minValue = MIN_FOV;
        fovSlider.maxValue = MAX_FOV;
        sensitivitySlider.minValue = MIN_SENSITIVITY;
        sensitivitySlider.maxValue = MAX_SENSITIVITY;
        sensitivitySlider.value = mouseSensitivity;
        rb.freezeRotation = true;
        playerProperties["voted"] = false;
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }

    private void SetCameraPosition()
    {
        cameraHolder.transform.position = cameraPos.position;
        usernameText.text = PhotonNetwork.NickName;
    }

    private void CameraLook()
    {
        if (gameIsPaused) return;
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.fixedDeltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.fixedDeltaTime;
        xRotation -= mouseY;
        yRotation += mouseX;
        orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
        playerCam.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
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
        
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (isGrounded)
            rb.AddForce(moveDirection.normalized * (moveSpeed * 10f), ForceMode.Force);
        else if (!isGrounded)
            rb.AddForce(moveDirection.normalized * (moveSpeed * 10f * airMultiplier), ForceMode.Force);
    }

    private void SpeedControl()
    {
        rb.drag = isGrounded ? 5f : 0f;
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(rb.transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void PauseGame()
    {
        if (gameIsPaused) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameIsPaused = true;
            playerProperties["gameIsPaused"] = gameIsPaused;
            PhotonNetwork.SetPlayerCustomProperties(playerProperties);
        }

    }

    public void ResumeGame()
    {
        if (!gameIsPaused) return;
        gameIsPaused = false;
        playerProperties["gameIsPaused"] = gameIsPaused;
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }

    private void HandlePauseMenu()
    {
        bool paused = (bool) playerProperties["gameIsPaused"];
        pauseMenuUI.SetActive(paused);
    }

    public void OnSliderFOVChanged()
    {
        CAMERA_FOV = fovSlider.value;
        playerProperties["cameraFOV"] = CAMERA_FOV;
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
        playerCam.fieldOfView = CAMERA_FOV;
    }

    private void FOVValueText()
    {
        if ((int) CAMERA_FOV == (int) NORMAL_FOV)
            fovValue.text = "Normal";
        else if ((int) CAMERA_FOV == (int) MAX_FOV)
            fovValue.text = "Quake Pro";
        else
            fovValue.text = string.Format("{0}", (int) CAMERA_FOV);
    }

    public void OnSliderMouseSensitivityChanged()
    {
        mouseSensitivity = sensitivitySlider.value;
        playerProperties["mouseSensitivity"] = mouseSensitivity;
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }

    private void MouseSensitivityValueText()
    {
        sensitivityValue.text = $"{(int) (sensitivitySlider.value / 4)}%";
    }

    private void ResetPlayer()
    {
        rb.transform.position = new Vector3(0, 4, 0);
    }
    
    
    

    private void FixedUpdate()
    {
        if (!PV.IsMine) return;
        MovePlayer();
    }

    private void Update()
    {
        if (!PV.IsMine) return;
        FOVValueText();
        MouseSensitivityValueText();
        PauseGame();
        HandlePauseMenu();
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.1f, groundMask);
        MyInput();  
        SpeedControl();
        SetCameraPosition();
        CameraLook();
        if (rb.transform.position.y < -30f) ResetPlayer();
    }
    
    
}
