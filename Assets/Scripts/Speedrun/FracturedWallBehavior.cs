using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FracturedWallBehavior : MonoBehaviour
{
    private PlayerMovement pm;
    private Rigidbody rb;
    public GameObject unbrokenWall;
    public GameObject brokenWall;
    public AudioSource glassBreaking;
    public float explosionRange, explosionForce, upwardsModifier;
    public LayerMask playerMask;
    public LayerMask enemyMask;
    public LayerMask glassMask;
    private void Awake()
    {
        pm = FindObjectOfType<PlayerMovement>();
        rb = pm.GetComponent<Rigidbody>();
        if (glassBreaking != null)
        {
            glassBreaking.loop = false;
            glassBreaking.playOnAwake = false;
            glassBreaking.mute = false;
        }
    }

    private void Start()
    {
        unbrokenWall.SetActive(true);
        brokenWall.SetActive(false);
    }

    private void Explosion()
    {
        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, 
            explosionRange, glassMask);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddExplosionForce(explosionForce, explosionPos, 
                    explosionRange, upwardsModifier, ForceMode.Impulse);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player") && rb.velocity.magnitude > 0f)
        {
            Explosion();
            if (glassBreaking != null)
            {
                glassBreaking.Play();
            }
            unbrokenWall.SetActive(false);
            brokenWall.SetActive(true);
        }
    }
}
