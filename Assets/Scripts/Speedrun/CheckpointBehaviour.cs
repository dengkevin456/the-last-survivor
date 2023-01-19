using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointBehaviour : MonoBehaviour
{
    private PlayerMovement pm;
    private void Awake()
    {
        pm = FindObjectOfType<PlayerMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "PlayerObj")
        {
            pm.nearestCheckpointPosition = transform.position;
        }
    }
}
