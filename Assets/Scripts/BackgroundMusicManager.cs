using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundMusicManager : MonoBehaviour
{
    [Header("Settings")] 
    [Tooltip("Delay in seconds")]
    public float delay;
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;
    private GameObject managerObject;
    public AudioSource selectedAudioSource;
    private void Awake()
    {
        managerObject = GetComponent<GameObject>();
    }

    private void Start()
    {
        selectedAudioSource.mute = true;
        selectedAudioSource.playOnAwake = false;
        selectedAudioSource.loop = true;
        selectedAudioSource.volume = musicVolume;
        Invoke(nameof(PlayDelay), delay);
    }

    private void PlayDelay()
    {
        selectedAudioSource.mute = false;
        selectedAudioSource.Play(); 
    }

    private void MatchGameSpeed()
    {
        selectedAudioSource.pitch = Time.timeScale;
    }


    private void Update()
    {
        if (PauseMenu.gameIsPaused) selectedAudioSource.Pause();
        else selectedAudioSource.UnPause();
        MatchGameSpeed();
    }
}
