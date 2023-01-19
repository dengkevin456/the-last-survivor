using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class SignText : MonoBehaviour
{
    [Header("References")]
    private Transform sign;
    private PlayerMovement pm;
    [SerializeField] private LayerMask signMask;
    private void Awake()
    {
        pm = FindObjectOfType<PlayerMovement>();
    }

    public void ReadSign()
    {
        if (Physics.Raycast(pm.cameraHolder.position, pm.cameraHolder.forward,
            out RaycastHit hit, 6f, signMask) && Input.GetKeyDown(KeyCode.T))
        {
            
        }
    }
}
