using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField roomInputField;
    public GameObject lobbyPanel;
    public GameObject roomPanel;
    public TMP_Text roomName;
    private PhotonView PV;
    
    public RoomItem roomItemPrefab;
    private List<RoomItem> roomItemsList = new List<RoomItem>();
    public Transform contentGroup;
    
    // Reducing lag
    public float timeBtwUpdates = 1.5f;
    private float nextUpdateTime;
    
    // Player items
    public List<PlayerItem> playerItemsList = new List<PlayerItem>();
    public PlayerItem playerItemPrefab;
    public Transform playerItemParent;
    
    // Load level
    public Button playButton;
    
    // Playground stuff
    public GameObject playGround;
    public GameObject playerPrefab;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        lobbyPanel.SetActive(true);
        playGround.SetActive(false);
        roomPanel.SetActive(false);
        PhotonNetwork.JoinLobby();
    }

    public void OnClickCreate()
    {
        if (roomInputField.text.Length >= 1)
        {
            PhotonNetwork.CreateRoom(roomInputField.text, new RoomOptions(){MaxPlayers = 3});
        }
    }

    public override void OnJoinedRoom()
    {
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);
        playGround.SetActive(true);
        PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0, 8, 0), Quaternion.identity);
        roomName.text = "Room: " + PhotonNetwork.CurrentRoom.Name;
        UpdatePlayerList();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (Time.time >= nextUpdateTime)
        {
            UpdateRoomList(roomList);
            nextUpdateTime = Time.time + timeBtwUpdates;
        }
    }

    private void UpdateRoomList(List<RoomInfo> list)
    {
        foreach (RoomItem item in roomItemsList)
        {
            Destroy(item.gameObject);
            
        }
        roomItemsList.Clear();

        foreach (RoomInfo room in list)
        {
            if (room.RemovedFromList) return;
            RoomItem newRoom = Instantiate(roomItemPrefab, contentGroup);
            newRoom.SetRoomName(room.Name);
            roomItemsList.Add(newRoom);
        }
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void OnClickLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }


    public override void OnLeftRoom()
    {
        roomPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        playGround.SetActive(false);
    }

    private void UpdatePlayerList()
    {
        foreach (PlayerItem item in playerItemsList)
        {
            Destroy(item.gameObject);
        }

        playerItemsList.Clear();

        if (PhotonNetwork.CurrentRoom == null)
        {
            return;
        }

        foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            PlayerItem newPlayerItem = Instantiate(playerItemPrefab, playerItemParent);
            newPlayerItem.SetPlayerInfo(player.Value);
            if (player.Value == PhotonNetwork.LocalPlayer)
            {
                newPlayerItem.ApplyLocalChanges();
            }
            playerItemsList.Add(newPlayerItem);
        }
    }

    // I will do exceptions later
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
    }


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // PhotonNetwork.Destroy(playerPrefab.GetComponent<PhotonView>());
        UpdatePlayerList();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            playButton.gameObject.SetActive(true);
            // Maximum players 1 for now
            playButton.interactable = PhotonNetwork.CurrentRoom.PlayerCount >= 1;
        }
        else
        {
            playButton.gameObject.SetActive(false);
        }
    }

    public void OnClickPlayButton()
    {
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        // Destroy(FindObjectOfType<PlaygroundPlayer>().gameObject);
        PhotonNetwork.LoadLevel("FPS");
    }
}
