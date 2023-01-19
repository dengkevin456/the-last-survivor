using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerItem : MonoBehaviour
{
    public TextMeshProUGUI playerName;
    public Color highLightColor;
    
    public void SetPlayerInfo(Player _player)
    {
        playerName.text = _player.NickName;
    }

    public void ApplyLocalChanges()
    {
        playerName.color = highLightColor;
    }
}
