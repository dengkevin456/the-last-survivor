using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public GameObject graphics;

    private void Awake()
    {
        graphics.SetActive(false);
    }
}
