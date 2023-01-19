using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class PauseMenu : MonoBehaviour
{
    [Header("Player references")] [SerializeField]
    // FOV stuff
    private Camera playerCam;
    public TextMeshProUGUI fovText;
    private PlayerMovement playerMovement;
    [Space] public TextMeshProUGUI volumeText;
    [Space] public TextMeshProUGUI mouseSensitivityText;
    [Space] public GameObject pauseMenuUI;
    [Space] public GameObject beatLevelUI;
    private Stopwatch stopWatch;
    [SerializeField] private Slider fovSlider;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider mouseSensitivitySlider;
    private int MIN_FOV = 30;
    private int MAX_FOV = 120;
    // Global player settings
    public static float CAMERA_FOV = 70f;
    public static float NORMAL_FOV = 70f;
    public static float NORMAL_VOLUME = 100f;
    public static float MASTER_VOLUME = 100f;
    public static float MOUSE_SENSITIVITY = 50f;
    public static float NORMAL_SENSITIVITY = 50f;
    public static bool gameIsPaused;
    public static bool levelComplete;
    // Level manager
    private LevelManager levelManager;

    private void Awake()
    {
        stopWatch = FindObjectOfType<Stopwatch>();
        playerMovement = FindObjectOfType<PlayerMovement>();
        levelManager = FindObjectOfType<LevelManager>();
    }

    private void Start()
    {
        CAMERA_FOV = NORMAL_FOV;
        MASTER_VOLUME = NORMAL_VOLUME;
        MOUSE_SENSITIVITY = NORMAL_SENSITIVITY;
        AudioListener.volume = MASTER_VOLUME / 100;
        playerCam.fieldOfView = CAMERA_FOV;
        gameIsPaused = false;
        levelComplete = false;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        fovSlider.value = CAMERA_FOV;
        fovSlider.minValue = MIN_FOV;
        fovSlider.maxValue = MAX_FOV;
        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 100f;
        volumeSlider.value = MASTER_VOLUME;
        mouseSensitivitySlider.value = MOUSE_SENSITIVITY;
        mouseSensitivitySlider.minValue = 10f;
        mouseSensitivitySlider.maxValue = 150f;
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        gameIsPaused = true;
    }

    public void NextLevel()
    {
        // TODO: Implement next level button
        LoadBestTime();
        // throw new NotImplementedException("Not implemented yet");
    }

    public void GoBack()
    {
        LoadBestTime();
        SceneManager.LoadScene(LevelStatistics.bestTimeDict.Count == LevelSelect.totalLevels
            ? "BeatGame"
            : "LevelSelect");
        Time.timeScale = 1f;
    }

    private void LoadBestTime()
    {
        if (!LevelStatistics.bestTimeDict.ContainsKey(LevelSelect.overallLevel))
        {
            LevelStatistics.bestTimeDict.Add(LevelSelect.overallLevel, stopWatch.currentTimeText.text);
            LevelStatistics.levelCompleteDict.Add(LevelSelect.overallLevel, true);
        }
        else
        {
            double parseOldTime = LevelStatistics.CalculateBestTime(stopWatch.currentTimeText.text);
            double parseNewTime = LevelStatistics.CalculateBestTime(
                LevelStatistics.bestTimeDict[LevelSelect.overallLevel]);
            double bestTime = parseOldTime;
            if (parseNewTime < parseOldTime) bestTime = parseNewTime;
            TimeSpan time = TimeSpan.FromSeconds(bestTime);
            LevelStatistics.bestTimeDict[LevelSelect.overallLevel] = time.ToString(@"mm\:ss\:ff");
        }
    }

    public void RestartGame()
    {
        Debug.Log("Reloading level...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        gameIsPaused = false;
        levelComplete = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        gameIsPaused = false;
    }

    private void HandleMenus()
    {
        if (!levelComplete)
        {
            if (gameIsPaused)
            {
                Cursor.lockState = CursorLockMode.None;
                pauseMenuUI.SetActive(true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                pauseMenuUI.SetActive(false);
            }
        }
        else if (!gameIsPaused)
        {
            if (levelComplete)
            {
                Cursor.lockState = CursorLockMode.None;
                stopWatch.StopStopwatch();
                beatLevelUI.SetActive(true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                beatLevelUI.SetActive(false);
            }
        }
    }


    public void ReturnToMainMenu()
    {
        Debug.Log("Returning to main menu...");
        SceneManager.LoadScene("MainMenu");
    }
    public void ReturnToMenu()
    {
        Debug.Log("Returning to menu...");
        SceneManager.LoadScene("LevelSelect");
    }
    public void OnFOVSliderValueChanged()
    {
        CAMERA_FOV = fovSlider.value;
    }

    public void OnMasterVolumeSliderChanged()
    {
        MASTER_VOLUME = volumeSlider.value;
    }

    public void OnMouseSensitivitySliderChanged()
    {
        MOUSE_SENSITIVITY = mouseSensitivitySlider.value;
    }

    private void ControlCameraFOV()
    {
        playerCam.fieldOfView = CAMERA_FOV;
        if ((int) CAMERA_FOV == 120)
            fovText.text = "Quake Pro";
        else if ((int) CAMERA_FOV == (int) NORMAL_FOV) 
            fovText.text = "Normal";
        else
            fovText.text = $"{(int) CAMERA_FOV}";
    }

    private void ControlMasterVolume()
    {
        AudioListener.volume = MASTER_VOLUME / 100;
        if ((int) MASTER_VOLUME == 100)
            volumeText.text = "All out!!!";
        else if ((int) MASTER_VOLUME == 50)
            volumeText.text = "Perfect volume";
        else if ((int) MASTER_VOLUME == 0)
            volumeText.text = "In a school classroom";
        else
            volumeText.text = $"{(int) MASTER_VOLUME}%";
    }

    private void ControlMouseSensitivity()
    {
        playerMovement.mouseSensitivity = MOUSE_SENSITIVITY * 4;
        if ((int) MOUSE_SENSITIVITY == (int) mouseSensitivitySlider.minValue)
            mouseSensitivityText.text = "*Yawn*";
        else if ((int) MOUSE_SENSITIVITY == (int) mouseSensitivitySlider.maxValue)
            mouseSensitivityText.text = "Gaming";
        else
            mouseSensitivityText.text = $"{(int) MOUSE_SENSITIVITY}%";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !gameIsPaused) PauseGame();
        ControlCameraFOV();
        ControlMasterVolume();
        ControlMouseSensitivity();
        HandleMenus();
    }
}
