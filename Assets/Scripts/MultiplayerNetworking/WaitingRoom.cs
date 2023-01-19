using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class WaitingRoom : MonoBehaviour
{
    public Button playAgainButton;
    private void Awake()
    {
        PhotonNetwork.CurrentRoom.IsVisible = true;
        PhotonNetwork.CurrentRoom.IsOpen = true;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        PhotonNetwork.Instantiate("PlaygroundPlayer", Vector3.zero, Quaternion.identity);
    }

    public void PlayGame()
    {
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel("FPS");
    }

    private void Update()
    {
        playAgainButton.interactable = PhotonNetwork.IsMasterClient;
    }
}
