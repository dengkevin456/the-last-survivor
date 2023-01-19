using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class PlayerController : MonoBehaviourPunCallbacks
{
    /// <summary>
    /// Note: The physics in the FPS player controller are not exactly the same as the parkour/playground controller
    /// You will feel more comfortable to play with physics in this version.
    /// </summary>
    private Rigidbody rb;
    private PhotonView PV;
    private TimeSpan t;
    [Header("References")] private RoomManager roomManager;
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private Camera playerCam;
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform cameraPos;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Item[] items;
    private int itemIndex;
    private int previousItemIndex = -1;
    private float horizontalInput;
    private float verticalInput;
    public Hashtable playerHash = new Hashtable();
    private float xRotation, yRotation;
    private Vector3 moveDirection;
    private bool isGrounded;
    [Header("Player settings")] public float mouseSensitivity = 400f;
    private float moveSpeed = 12f;
    public float normalSpeed = 7f;
    public float sprintSpeed = 18f;
    public float jumpForce = 5f;
    public float jumpCoolDown;
    private bool readyToJump;
    private bool isSprinting;
    [Header("Player inputs")] public KeyCode jumpKey = KeyCode.Space;
    [Header("UI stuff")] [SerializeField] private Image healthbarImage;
    public GameObject gameUI;
    public GameObject deathUI;
    public GameObject pauseMenuUI;
    public GameObject leaderboardContentUI;
    public TextMeshProUGUI playerLeaderboardText;
    [Tooltip("The current item the player is holding")]
    [Space]
    public TextMeshProUGUI currentItemText;

    [Tooltip("The death message of the player being killed")] [Space]
    public TextMeshProUGUI killMessage;
    public List<TextMeshProUGUI> playerEntries = new List<TextMeshProUGUI>();
    public Button returnToLobbyButton;
    public GameObject gameOverUI;
    public TextMeshProUGUI timerLeftText;
    private bool gameIsPaused;
    private bool gameOver;
    public AudioListener audioListener;
    private const float maxHealth = 100f;
    [HideInInspector]
    public float currentHealth = 100f;

    [HideInInspector] public PlayerManager playerManager;
    // Gravity stuff
    private Vector3 normalGravity = new Vector3(.0f, -9.81f, .0f);
    private void Awake()
    {
        Physics.gravity = normalGravity;
        rb = GetComponentInChildren<Rigidbody>();
        PV = GetComponent<PhotonView>();
        playerManager = PhotonView.Find((int) PV.InstantiationData[0]).GetComponent<PlayerManager>();
        pauseMenuUI.SetActive(false);
    }

    private bool isGameContinued()
    {
        return gameIsPaused || gameOver;
    }

    private void Start()
    {
        if (!PV.IsMine)
        {
            Destroy(audioListener);
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(timerLeftText.gameObject);
            Destroy(gameOverUI);
            Destroy(gameUI);
            Destroy(deathUI);
            Destroy(rb);
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            if (currentItemText != null) currentItemText.gameObject.SetActive(true);
            deathUI.SetActive(false);
            roomManager = FindObjectOfType<RoomManager>();
            gameUI.SetActive(true);
            pauseMenuUI.SetActive(false);
            gameOverUI.SetActive(false);
            readyToJump = true;
            cameraHolder.position = cameraPos.position;
            playerHash = PV.Controller.CustomProperties;
            playerHash["gameIsPaused"] = gameIsPaused;
            playerHash["gameOver"] = gameOver;
            playerHash["itemIndex"] = 0;
            playerCam.fieldOfView = (float) playerHash["cameraFOV"];
            EquipItem(0);
            for (int i = 0; i < roomManager.playerManagerList.Count; i++)
            {
                TextMeshProUGUI newPlayerLeadText = Instantiate(playerLeaderboardText,
                    leaderboardContentUI.transform);
                playerEntries.Add(newPlayerLeadText);
            }
        }

        PhotonNetwork.SetPlayerCustomProperties(playerHash);
    }

    private void LookCamera()
    {
        if (isGameContinued()) return;
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.fixedDeltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.fixedDeltaTime;
        xRotation -= mouseY;
        yRotation += mouseX;
        cameraHolder.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0f);
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
    }

    private void MyInput()
    {
        if (isGameContinued()) return;
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        if (Input.GetKey(jumpKey) && readyToJump && isGrounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCoolDown);
        }
    }

    private void StateHandler()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !isSprinting && Input.GetKey(KeyCode.W))
        {
            isSprinting = true;
        }

        if (Input.GetKeyUp(KeyCode.Q)) isSprinting = false;


        moveSpeed = isSprinting ? sprintSpeed : normalSpeed;
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        rb.AddForce(moveDirection.normalized * (moveSpeed * 10f), ForceMode.Force);
        // rb.MovePosition(rb.position + rb.transform.TransformDirection(moveDirection.normalized * moveSpeed) * Time.fixedDeltaTime);
        
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void PauseGame()
    {
        if (isGameContinued()) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            gameIsPaused = true;
            playerHash["gameIsPaused"] = gameIsPaused;
            PhotonNetwork.SetPlayerCustomProperties(playerHash);
        }
    }

    public void ResumeGame()
    {
        if (!gameIsPaused && !gameOver) return;
        Cursor.lockState = CursorLockMode.Locked;
        gameIsPaused = false;
        playerHash["gameIsPaused"] = gameIsPaused;
        PhotonNetwork.SetPlayerCustomProperties(playerHash);
    }

    public void QuitGame()
    {
        playerManager.QuitGame();
    }

    private void HandlePauseMenu()
    {
        bool paused = (bool) playerHash["gameIsPaused"];
        pauseMenuUI.SetActive(paused);
    }

    private void HandleGameOverMenu()
    {
        bool gameOver = (bool) playerHash["gameOver"];
        gameOverUI.SetActive(gameOver);
        returnToLobbyButton.interactable = PhotonNetwork.IsMasterClient;

    }

    public void ReturnToLobby()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("You are not the master client!");
            return;
        }

        PhotonNetwork.LoadLevel("WaitingRoom");
    }

    private void EquipItem(int _index)
    {
        if (_index == previousItemIndex) return;
        itemIndex = _index;
        items[itemIndex].itemGameObject.SetActive(true);
        if (previousItemIndex != -1)
        {
            items[previousItemIndex].itemGameObject.SetActive(false);
        }

        if (currentItemText != null)
        {
            StartCoroutine(FadeText(1.5f, 2f));   
        }
        previousItemIndex = itemIndex;
        if (PV.IsMine)
        {
            playerHash["itemIndex"] = itemIndex;
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
        }
    }

    private IEnumerator FadeText(float delay, float duration)
    {
        currentItemText.faceColor = new Color32(0, 0, 0, 255);
        yield return new WaitForSeconds(delay);
        currentItemText.CrossFadeAlpha(0, duration, true);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        bool containsKeys = 
            changedProps.ContainsKey("itemIndex") && 
            changedProps.ContainsKey("gameOver") && 
            changedProps.ContainsKey("gameIsPaused");
        if (changedProps.ContainsKey("itemIndex") && !PV.IsMine && targetPlayer == PV.Owner)
        {
            EquipItem((int) changedProps["itemIndex"]);
            
        }
    }

    public void TakeDamage(float damage)
    {
        PV.RPC(nameof(RPC_TakeDamage), RpcTarget.All, damage);
    }
    
    [PunRPC]
    public void RPC_TakeDamage(float damage, PhotonMessageInfo info)
    {
        if (!PV.IsMine) return;
        currentHealth -= damage;
        healthbarImage.fillAmount = currentHealth / maxHealth;
        if (currentHealth <= 0)
        {
            rb.transform.position = Vector3.forward * 10000;
            rb.isKinematic = true;
            Die();
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            PlayerManager.Find(info.Sender).GetKill();
            deathUI.SetActive(true);
            if (killMessage != null)
            {
                killMessage.text = $"You were killed by {info.Sender.NickName}";
            }
        }
    }

    public void RespawnPlayer()
    {
        Debug.Log("Respawning...");
        Respawn();
    }

    private void Die()
    {
        playerManager.Die();
    }

    private void Respawn()
    {
        playerManager.Respawn();
    }

    private void SetLeaderboard(List<PlayerManager> playerManagerList)
    {
        for (int i = 0; i < playerEntries.Count; i++)
        {
            
            playerEntries[i].text = $"{i + 1}. " +
                                    $"[{PhotonNetwork.NickName}] " + 
                                    $"(Kills: {playerManagerList[i].kills}) " +
                                    $"(Deaths: {playerManagerList[i].deaths})";
        }
    }

    private void SpeedControl()
    {
        if (!PV.IsMine) return;
        rb.drag = isGrounded ? 5f : 0f;
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }   
    }

    private void FixedUpdate()
    {
        if (!PV.IsMine) return;
        if (isGameContinued()) return;
        MovePlayer();
        
    }

    private void Update()
    {
        if (!PV.IsMine) return;
        t = TimeSpan.FromSeconds(roomManager.timer);
        timerLeftText.text = $"{t:mm\\:ss}";
        isGrounded = Physics.CheckSphere(groundCheck.position, .1f, groundMask);
        cameraHolder.position = cameraPos.position;
        StateHandler();
        HandlePauseMenu();
        HandleGameOverMenu();
        SpeedControl();
        PauseGame();
        MyInput();
        LookCamera();

        for (int i = 0; i < items.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                EquipItem(i);
                break;
            }
        }
        // If timer hits 0
        if (roomManager.timer <= 0)
        {
            gameOver = true;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            playerHash["gameOver"] = gameOver;
            PhotonNetwork.SetPlayerCustomProperties(playerHash);
            SetLeaderboard(roomManager.playerManagerList);
        } 
        
        // Mouse scrollwheel
        if (Input.GetAxisRaw("Mouse ScrollWheel") > .0f)
        {
            if (itemIndex >= items.Length - 1) EquipItem(0);
            else EquipItem(itemIndex + 1);
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < .0f)
        {
            if (itemIndex <= 0) EquipItem(items.Length - 1);
            else EquipItem(itemIndex - 1);
        }

        if (!isGameContinued())
        {
            items[itemIndex].Use();
            items[itemIndex].AlternateUse();
            if (currentItemText.text != null)
            {
                // currentItemText.gameObject.SetActive(true);
                currentItemText.text = items[itemIndex].itemInfo.itemName;   
            }
        }

        if (rb.position.y < -40f)
        {
            Die();
            Respawn();
        }
    }
}
