using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Settings")]
    public int level = 1;
    [Space]
    [Tooltip("Check if it is the last playable level")]
    public Transform player;
    private Rigidbody playerRB;
    public LayerMask checkPointMask;
    public Transform[] checkPoints;
    // First index: Checkpoint distance, second index: the index of checkpoint
    private float[] nearestCheckpoint = new float[2];

    private void Awake()
    {
        playerRB = player.GetComponentInChildren<Rigidbody>();
        nearestCheckpoint[0] = 0;
        nearestCheckpoint[1] = 0;
    }

    private void ResetPlayer()
    {
        if (playerRB.position.y < -70 && level != 1)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void Update()
    {
        ResetPlayer();
    }
}
