using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class MainMenuConfig : MonoBehaviour
{
    public void MultiplayerScreen()
    {
        Debug.Log("Going to server screen...");
        SceneManager.LoadScene("ConnectToServer");
    }

    public void OfflineScreen()
    {
        Debug.Log("Going to offline screen...");
        SceneManager.LoadScene("LevelSelect");
    }

    public void AboutMe()
    {
        SceneManager.LoadScene("AboutMe");
    }
}
