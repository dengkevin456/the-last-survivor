using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class UsernameDisplay : MonoBehaviour
{
    [SerializeField] private PhotonView PV;
    [SerializeField] private TMP_Text text;
    private void Start()
    {
        if (PV.IsMine)
        {
            gameObject.SetActive(false);
        }
        text.text = PV.Owner.NickName;
    }
}
