using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
public class BulletBehaviour : MonoBehaviour
{
    // TODO: Syncing bullet over the network
    [Header("References")]
    private Transform bullet;

    private Rigidbody rb;
    private PhotonView PV;
    public GameObject bulletImpactPrefab;
    [Space]
    public LayerMask groundMask, playerMask;
    public bool isTrigger = true;
    [HideInInspector]
    public MultiplayerGun bulletGun;

    private RaycastHit hit;
    private void Awake()
    {
        bullet = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        StartCoroutine(Predict());
    }
    

    private IEnumerator Predict()
    {
        Vector3 prediction = transform.position + rb.velocity * Time.fixedDeltaTime;
        RaycastHit hit2;
        int layermask = LayerMask.GetMask("Bullet");
        if (Physics.Linecast(transform.position, prediction, out hit2))
        {
            transform.position = hit2.point;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.isKinematic = true;
            PV.RPC(nameof(RPC_Shoot), RpcTarget.All, hit2.point, hit2.normal);
            OnTriggerEnterFixed(hit2.collider);
            yield return 0;
        }
    }

    private void OnTriggerEnterFixed(Collider other)
    {
        if (!PV.IsMine) return;
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponentInParent<PlayerController>().TakeDamage(bulletGun.damage);
        }
        PhotonNetwork.Destroy(bullet.gameObject);
    }


    [PunRPC]
    private void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPosition, .1f);
        if (colliders.Length != 0)
        {
            GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hitPosition + hitNormal * 0.001f, 
                Quaternion.LookRotation(hitNormal, Vector3.up) * 
                bulletImpactPrefab.transform.rotation);
            Destroy(bulletImpactObj, 10f);
            bulletImpactObj.transform.SetParent(colliders[0].transform);
        }
    }

    private void FixedUpdate()
    {
        StartCoroutine(Predict());
    }
}
