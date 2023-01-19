using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public abstract class MultiplayerGun : Item
{
    public PlayerController pc;
    public Camera playerCam;
    public Rigidbody bulletPrefab;
    public Transform gunTip;
    public float shootForce;
    public float damage = 10f;
    public Animation shootAnimation;
    public TextMeshProUGUI ammoText;
    [HideInInspector] public bool isAiming;

    private void Start()
    {
        shootAnimation.playAutomatically = false;
    }

    public abstract override void Use();

    public IEnumerator DestroyBulletDelay(float seconds, GameObject bullet)
    {
        yield return new WaitForSeconds(seconds);
        if (PhotonNetwork.IsMasterClient && bullet)
            PhotonNetwork.Destroy(bullet);
    }
    public abstract override void AlternateUse();
}
