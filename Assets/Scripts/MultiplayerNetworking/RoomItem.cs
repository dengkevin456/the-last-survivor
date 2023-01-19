using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class RoomItem : MonoBehaviour
{
    public TMP_Text roomName;
    private LobbyManager manager;
    public void SetRoomName(string _roomName)
    {
        roomName.text = _roomName;
    }

    private void Start()
    {
        manager = FindObjectOfType<LobbyManager>();
    }

    public void OnClickItem()
    {
        manager.JoinRoom(roomName.text);
    }
}
