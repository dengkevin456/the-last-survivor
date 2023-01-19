using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBullet : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerMovement pm;
    private LevelManager levelManager;
    public float bulletSpeed = 5f;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pm = FindObjectOfType<PlayerMovement>();
        levelManager = FindObjectOfType<LevelManager>();
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
            OnTriggerEnterFixed(hit2.collider);
            yield return 0;
        }
    }

    private void OnTriggerEnterFixed(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (levelManager.level == 1) pm.CheckpointRespawn();
            else pm.Die();
        }
        Destroy(gameObject);
    }

    public void Shoot(Vector3 direction)
    {
        rb.AddForce(direction.normalized * bulletSpeed, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        if (PauseMenu.gameIsPaused) return;
        StartCoroutine(Predict());
    }
}
