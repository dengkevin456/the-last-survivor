using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ScoreBoardItem : MonoBehaviourPunCallbacks
{
    public TMP_Text usernameText;
    public TMP_Text killstext;
    public TMP_Text deathsText;
    private Player player;
    public void Initialize(Player player)
    {
        this.player = player;
        usernameText.text = player.NickName;
        UpdateStats();
    }

    private void UpdateStats()
    {
        if (player.CustomProperties.TryGetValue("kills", out var kills))
        {
            killstext.text = kills.ToString();
        }
        if (player.CustomProperties.TryGetValue("deaths", out var deaths))
        {
            deathsText.text = deaths.ToString();
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (targetPlayer == player)
        {
            if (changedProps.ContainsKey("kills") || changedProps.ContainsKey("deaths"))
            {
                UpdateStats();
            }
        }
    }
}
