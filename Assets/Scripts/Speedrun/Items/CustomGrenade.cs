using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomGrenade : MonoBehaviour
{
    [Header("Refs")]
    private Rigidbody rb;

    private ParticleSystem explosiveGrenade;
    private Collider customGrenadeCollider;
    // public GameObject explosion;
    public LayerMask whatIsEnemy;
    public LayerMask groundMask;
    public LayerMask explosiveMask;
    public enum CollisionType
    {
        Duration,
        OnContact,
        Bounce
    }

    public CollisionType collisionType = CollisionType.Duration;
    
    [Header("Stats")]
    [Range(0f, 1f)] public float bounciness;
    public bool useGravity;

    [Header("Damage")] public int explosionDamage;
    public float explosionRange;
    public float explosionForce;
    public float upwardsModifier;

    [Header("Lifetime")] public int maxCollisions;
    public float maxLifeTime;
    public bool explosionOnTouch = true;
    private int collisions;
    private PhysicMaterial physics_mat;
    // private bool _isexplosionNotNull;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (explosiveGrenade != null)
        {
            explosiveGrenade = GetComponentInChildren<ParticleSystem>();
        }

        customGrenadeCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        // _isexplosionNotNull = explosion != null;
        Setup();
    }

    private void Setup()
    {
        physics_mat = new PhysicMaterial();
        physics_mat.bounciness = bounciness;
        physics_mat.frictionCombine = PhysicMaterialCombine.Minimum;
        physics_mat.bounceCombine = PhysicMaterialCombine.Maximum;
        customGrenadeCollider.material = physics_mat;
        rb.useGravity = useGravity;
    }

    private void Explode()
    {
        // if (_isexplosionNotNull) Instantiate(explosion, transform.position, Quaternion.identity);
        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRange);
        for (int i = 0; i < enemies.Length; i++)
        {
            Rigidbody staticRB = enemies[i].GetComponentInParent<Rigidbody>();
            /*
             if (enemies[i].GetComponentInChildren<Rigidbody>())
            {
                enemies[i].GetComponentInChildren<Rigidbody>().AddExplosionForce(explosionForce,
                    transform.position, explosionRange, upwardsModifier);
                if (enemies[i].GetComponentInChildren<EnemyAI>())
                {
                    enemies[i].GetComponentInChildren<EnemyAI>().DamageHealth(this);
                    Debug.Log("Damaging enemy AI...");
                }
                
                
            }
            
            else
             
             */
            if (staticRB)
            {
                Debug.Log("Added the player!");
                enemies[i].GetComponentInParent<Rigidbody>().AddExplosionForce(explosionForce,
                    transform.position, explosionRange, upwardsModifier, ForceMode.Impulse);
            }
            
            if (enemies[i].GetComponentInParent<CannonEnemy>())
            {
                enemies[i].GetComponentInParent<CannonEnemy>().TakeDamage(30f);
            }
        }
    }

    private void Delay()
    {
        Destroy(gameObject);
    }


    private void OnCollisionEnter(Collision other)
    {
        if (collisionType == CollisionType.OnContact)
        {
            if (other.gameObject.layer != explosiveMask)
            {
                Debug.Log("Collision!");
                if (other.gameObject.CompareTag("Player"))
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                if (other.gameObject.CompareTag("Enemy"))
                {
                    if (other.gameObject.GetComponent<EnemyAI>() != null)
                    {
                        EnemyAI enemyAI = other.gameObject.GetComponentInParent<EnemyAI>();
                        enemyAI.health = 0;   
                    }
                }
                Explode();
            }

            if (explosiveGrenade != null)
            {
                explosiveGrenade.Play();
            }
            Destroy(gameObject, 0.6f); 
        }
    }

    private void Update()
    {
        // if (collisions > maxCollisions) Explode();
        if (collisionType == CollisionType.Duration)
        {
            maxLifeTime -= Time.deltaTime;
            if (maxLifeTime <= 0) Explode();   
        }
    }
    
}
