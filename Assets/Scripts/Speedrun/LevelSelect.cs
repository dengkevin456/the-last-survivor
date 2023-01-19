using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    private VerticalLayoutGroup levelUI;
    // Important: change this
    public static int totalLevels = 3;
    public static int overallLevel = 1;
    public int level;
    public TextMeshProUGUI bestTimeText;
    private bool levelUnlocked = false;
    private bool levelComplete = false;
    private float bestTime;

    private void Start()
    {
        levelUI = GetComponent<VerticalLayoutGroup>();
    }

    public void GoBack()
    {
        Debug.Log("Going to main menu...");
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadLevel()
    {
        Debug.Log($"Loading level {level}...");
        SceneManager.LoadScene($"Level {level}");
        overallLevel = level;
    }

    private void HandleLevelComplete()
    {
        if (!LevelStatistics.levelCompleteDict.ContainsKey(level)) return;
        levelComplete = LevelStatistics.levelCompleteDict[level];
    }

    private void HandleBestTime()
    {
        if (!levelComplete)
        {
            bestTimeText.text = "NaN";
        }
        else
        {
            if (LevelStatistics.levelCompleteDict.ContainsKey(level))
                bestTime = (float) LevelStatistics.CalculateBestTime(LevelStatistics.bestTimeDict[level]);
            bestTimeText.text = "Best time: " + TimeSpan.FromSeconds(bestTime).ToString(@"mm\:ss\:ff");
        }
    }

    private void Update()
    {
        HandleLevelComplete();
        HandleBestTime();
    }
}