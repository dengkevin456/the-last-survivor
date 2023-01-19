using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;
    private bool inGame;
    [SerializeField] private float origTimer = 5f;
    [HideInInspector]
    public float timer = 5f;
    [HideInInspector]
    public List<PlayerManager> playerManagerList = new List<PlayerManager>();
    public enum GameMode
    {
        TeamDeathMatch,
        FreeForAll,
        Point,
        CaptureTheFlag
    }
    private void Awake()
    {
        // roomHash = PhotonNetwork.CurrentRoom.CustomProperties;
        // DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    private void Start()
    {
        playerManagerList.Clear();
        Physics.gravity = new Vector3(0, -9.81f, 0);
        timer = origTimer;
        GameObject newPlayer = PhotonNetwork.Instantiate("PlayerManager", Vector3.zero, Quaternion.identity);
        playerManagerList.Add(newPlayer.GetComponent<PlayerManager>());
        inGame = true;
    }

    

    private void Update()
    {
        if (inGame)
        {
            playerManagerList.Sort((x, y) 
                => x.kills.CompareTo(y.kills));
            if (timer > 0f)
            {
                timer -= Time.deltaTime;
            }
        }
    }
}
