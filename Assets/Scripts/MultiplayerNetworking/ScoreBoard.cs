using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class ScoreBoard : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform container;
    [SerializeField] private GameObject scoreBoardItemPrefab;
    private CanvasGroup canvasGroup;

    private Dictionary<Player, ScoreBoardItem> scoreBoardItems = new Dictionary<Player, ScoreBoardItem>();

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        canvasGroup.alpha = 0;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            AddScoreBoardItem(player);
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AddScoreBoardItem(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RemoveScoreBoardItem(otherPlayer);
    }
    private void AddScoreBoardItem(Player player)
    {
        ScoreBoardItem item = Instantiate(scoreBoardItemPrefab, container).GetComponent<ScoreBoardItem>();
        item.Initialize(player);
        scoreBoardItems[player] = item;
    }

    private void RemoveScoreBoardItem(Player player)
    {
        Destroy(scoreBoardItems[player].gameObject);
        scoreBoardItems.Remove(player);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            canvasGroup.alpha = 1;
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            canvasGroup.alpha = 0;
        }
    }
}
