using Photon.Pun;
using UnityEngine;

public class LavaCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        PlayerController pc = other.collider.GetComponentInParent<PlayerController>();
        PhotonView PV = pc.GetComponent<PhotonView>();
        PlayerManager playerManager = PhotonView.Find((int) PV.InstantiationData[0]).GetComponent<PlayerManager>();
        playerManager.Die();
        playerManager.Respawn();
    }
}
