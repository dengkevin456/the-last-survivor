using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using LootLocker.Requests;
using UnityEngine.UI;

public class LeaderboardController : MonoBehaviour
{
    public TMP_InputField MemberID;
    public TextMeshProUGUI overallTimeText;
    public int ID;
    private int maxScores = 20;
    public TextMeshProUGUI[] Entries;
    private double playerScore = 0;
    public GameObject congratulationsUI;
    public GameObject leaderboardUI;
    public Button submitButton;

    private void Awake()
    {
        submitButton.interactable = true;
        congratulationsUI.SetActive(true);
        leaderboardUI.SetActive(false);
        LootLockerSDKManager.StartGuestSession(response =>
        {
            if (response.success)
            {
                Debug.Log("Success!");
            }
            else
            {
                Debug.LogError("<color=black>Error:</color>Failed to start Lootlocker session");
            }
        });
        foreach (KeyValuePair<int, string> entry in LevelStatistics.bestTimeDict)
        {
            playerScore += LevelStatistics.CalculateBestTime(entry.Value);
        }
        overallTimeText.text = $"Overall time: {ConvertTime(playerScore)}";
    }

    private string ConvertTime(double totalTime)
    {
        TimeSpan time = TimeSpan.FromSeconds(totalTime);
        return time.ToString(@"mm\:ss\:ff");
    }

    public void OpenFormsURL()
    {
        Application.OpenURL(AboutMe.formsURL);
    }

    public void RefreshScore()
    {
        LootLockerSDKManager.GetScoreList(ID, maxScores, response =>
        {
            if (response.success)
            {
                LootLockerLeaderboardMember[] scores = response.items;

                for (int i = 0; i < scores.Length; i++)
                {
                    Entries[i].text = $"{scores[i].rank} {scores[i].member_id}: {scores[i].score}s";
                }

                if (scores.Length < maxScores)
                {
                    for (int i = scores.Length; i < maxScores; i++)
                    {
                        Entries[i].text = $"{i + 1}. NaN";
                    }
                }
            }
            else
            {
                Debug.LogError("<color=black>Error:</color>Failed to start Lootlocker session");
            }
        });
    }

    public void LeaderboardSubmission()
    {
        congratulationsUI.SetActive(false);
        leaderboardUI.SetActive(true);
    }

    public void SubmitScore()
    {
        LootLockerSDKManager.SubmitScore(MemberID.text, (int) playerScore, 
            ID, response =>
            {
                if (response.success)
                {
                    Debug.Log("Success!");
                }
                else
                {
                    Debug.LogError("<color=red>Error:</color>Failed to start Lootlocker session");
                }
            });
        submitButton.interactable = false;
    }
    
}
