using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CannonEnemy : MonoBehaviour
{
    [Header("References")] public GameObject aliveCannon;
    public GameObject deadCannon;
    public AudioSource gunSound;
    [Space]
    public float health = 100f;
    public Vector3 rotateAmount;
    private NavMeshAgent agent;
    public Rigidbody cannonBullet;
    public Transform turret;
    public Transform turretTip;
    private PlayerMovement pm;
    private bool isAlive;
    public float turningSpeed = 10f;
    public float sightRange = 20f;
    public float attackRange = 10f;
    public bool playerInSightRange, playerInAttackRange;
    public Vector3 walkPoint;
    private bool walkPointSet;
    public float walkPointRange;

    [Header("Attacking")] public float timeBetweenAttacks;
    private bool alreadyAttacked;
    private void Start()
    {
        isAlive = true;
        agent = GetComponentInChildren<NavMeshAgent>();
        pm = FindObjectOfType<PlayerMovement>();
        aliveCannon.SetActive(true);
        deadCannon.SetActive(false);
    }

    private void LookAtPlayer(Transform target)
    {
        var lookPos = target.transform.position - turret.transform.position;
        Quaternion lookRot = Quaternion.LookRotation(lookPos);
        lookRot.eulerAngles = new Vector3(turret.transform.rotation.eulerAngles.x, 
            lookRot.eulerAngles.y, 180 - turret.transform.rotation.eulerAngles.z);
        turret.transform.rotation = Quaternion.Slerp(turret.transform.rotation, lookRot, 
            Time.deltaTime * turningSpeed);
    }
    
    // AI states
    
    // Patroling
    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();
        if (walkPointSet) agent.SetDestination(walkPoint);
        Vector3 distance = transform.position - walkPoint;
        if (distance.magnitude < 1f)
            walkPointSet = false;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(pm.groundCheck.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(pm.groundCheck.position);
        LookAtPlayer(pm.cameraPos);
        if (!alreadyAttacked)
        {
            gunSound.Play();
            // Attack code
            Rigidbody newCannonBullet = Instantiate(cannonBullet, turretTip.position, Quaternion.identity);
            newCannonBullet.rotation = turretTip.rotation;
            newCannonBullet.gameObject.SetActive(true);

            Vector3 direction = pm.cameraPos.position - turret.position;
            newCannonBullet.transform.forward = direction.normalized;
            newCannonBullet.GetComponent<CannonBullet>().Shoot(direction);
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);            
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        Vector3 at = agent.transform.position;
        walkPoint = new Vector3(at.x + randomX, at.y, at.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, LayerMask.GetMask("Ground")))
            walkPointSet = true;
    }
    
    // Damage cannon

    public void TakeDamage(float damage)
    {
        Debug.Log("Took " + damage + " damage!");
        health -= damage;
    }

    private void Die()
    {
        Destroy(agent);
        deadCannon.transform.position = aliveCannon.transform.position;
        aliveCannon.SetActive(false);
        deadCannon.SetActive(true);
    }

    private void Update()
    {
        if (PauseMenu.gameIsPaused) return;
        if (isAlive)
        {
            playerInSightRange = Physics.CheckSphere(agent.transform.position,
                sightRange, LayerMask.GetMask("Player"));
            playerInAttackRange = Physics.CheckSphere(agent.transform.position, attackRange,
                LayerMask.GetMask("Player"));
            if (!playerInSightRange && !playerInAttackRange)
            {
                agent.stoppingDistance = 0f;
                transform.Rotate(rotateAmount * Time.deltaTime);
                Patroling();
            }
            if (playerInSightRange && !playerInAttackRange)
            {
                agent.stoppingDistance = 0f;
                LookAtPlayer(pm.cameraPos);
                ChasePlayer();
            }

            if (playerInSightRange && playerInAttackRange)
            {
                agent.stoppingDistance = 5f;
                AttackPlayer();
            }

            if (health <= 0) isAlive = false;
        }
        
        if (!isAlive) Die();
    }
}
