using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour
{
    private Animator animator;
    public GameObject aliveRobot, deadRobot;
    public NavMeshAgent agent;
    public float health = 100f;
    protected PlayerMovement pm;
    public GameObject arrowHolder;
    public LayerMask playerMask;
    public LayerMask groundMask;
    public float trajectoryHeight = 5f;

    public Rigidbody arrow;
    // Patroling
    public Vector3 walkPoint;
    private bool walkPointSet;
    public float walkPointRange;
    
    // Type of enemy (for now, for debugging purposes, let's just set it as an enum)
    public enum EnemyType
    {
        Arrow,
        Grappler,
    }

    public EnemyType enemyType = EnemyType.Arrow;
    
    //Attacking
    public float timeBetweenAttacks;
    private bool alreadyAttacked;
    
    // States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;
    private static readonly int IsChasing = Animator.StringToHash("isChasing");
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        arrow.gameObject.SetActive(false);
    }

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        pm = FindObjectOfType<PlayerMovement>();
        deadRobot.SetActive(false);
    }

    private void Update()
    {
        if (PauseMenu.gameIsPaused) return;
        if (health > 0)
        {
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange, playerMask);
            playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerMask);

            if (!playerInSightRange && !playerInAttackRange)
            {
                animator.SetBool(IsChasing, false);
                animator.SetBool(IsAttacking, false);
                Patroling();
            }

            if (playerInSightRange && !playerInAttackRange)
            {
                animator.SetBool(IsChasing, true);
                animator.SetBool(IsAttacking, false);
                ChasePlayer();
            }

            if (playerInSightRange && playerInAttackRange)
            {
                animator.SetBool(IsChasing, false);
                animator.SetBool(IsAttacking, true);
                AttackPlayer();
            }
        }

        Die();
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();
        if (walkPointSet)
            agent.SetDestination(walkPoint);
        Vector3 distance = transform.position - walkPoint;
        if (distance.magnitude < 1f)
            walkPointSet = false;   
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, groundMask))
            walkPointSet = true;
        
    }

    private void ChasePlayer()
    {
        agent.SetDestination(pm.transform.position);
    }

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));
        return velocityXZ + velocityY;
    }

    private void ShootPlayerWithArrow(Vector3 targetPosition, float trajectoryHeight)
    {
        Rigidbody newArrow = Instantiate(arrow, arrowHolder.transform.position, Quaternion.identity);
        newArrow.gameObject.SetActive(true);
        newArrow.velocity = CalculateJumpVelocity(arrowHolder.transform.position, targetPosition, trajectoryHeight);
    }
    

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        transform.LookAt(pm.transform.position);
        if (!alreadyAttacked)
        {
            if (enemyType == EnemyType.Arrow)
            {
                ShootPlayerWithArrow(pm.transform.position, trajectoryHeight);
            }
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    public void DamageHealth(CustomGrenade customGrenade)
    {
        health -= customGrenade.explosionDamage;
    }

    private void Die()
    {
        if (health <= 0)
        {
            health = 0;
            Destroy(agent);
            aliveRobot.SetActive(false);
            Destroy(animator);
            deadRobot.SetActive(true);
        }
    }

    private void ResetAttack() 
    {
        alreadyAttacked = false;
        
    }
}
