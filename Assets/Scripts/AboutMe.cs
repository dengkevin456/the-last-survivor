using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class AboutMe : MonoBehaviour
{
    public static string formsURL = "https://forms.office.com/r/FbVG9YC3m5";
    public VideoPlayer videoPlayer;

    private void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Play();
            videoPlayer.isLooping = true;
        }
    }

    public void GoBack()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenFormsURL()
    {
        Debug.Log("Opening suggestions url...");
        Application.OpenURL(formsURL);
    }
}
