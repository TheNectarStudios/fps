using UnityEngine;
using UnityEngine.AI;

public class EnemyAIController : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 20f;
    public float aimRange = 15f;
    public float shootingRange = 10f;
    public float fieldOfView = 60f;

    private Animator animator;
    private NavMeshAgent agent;
    private bool isAiming = false;
    private bool isShooting = false;
    private bool isWalking = false;

    private Vector3 lastKnownPlayerPosition;
    private bool playerInSight = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if player is within detection range and field of view
        playerInSight = IsPlayerInSight();

        // Update Animator parameters based on conditions
        animator.SetBool("isPlayerNear", distanceToPlayer <= detectionRange);
        animator.SetBool("canSeePlayer", playerInSight);
        
        if (distanceToPlayer <= shootingRange && playerInSight)
        {
            StartShooting();
        }
        else if (distanceToPlayer <= aimRange && playerInSight)
        {
            StartAiming();
        }
        else if (distanceToPlayer <= detectionRange && !playerInSight)
        {
            StartWalkingToLastKnownPosition();
        }
        else
        {
            GoIdle();
        }

        // Handle aiming direction if aiming
        if (isAiming && playerInSight)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
            animator.SetFloat("aimDirection", angle);
        }
    }

    private bool IsPlayerInSight()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (angleToPlayer < fieldOfView / 2)
        {
            Ray ray = new Ray(transform.position, directionToPlayer);
            if (Physics.Raycast(ray, out RaycastHit hit, detectionRange))
            {
                if (hit.transform == player)
                {
                    lastKnownPlayerPosition = player.position;
                    return true;
                }
            }
        }
        return false;
    }

    private void StartAiming()
    {
        isAiming = true;
        isShooting = false;
        isWalking = false;

        animator.SetBool("isAiming", true);
        animator.SetBool("isShooting", false);
        animator.SetBool("isWalking", false);

        agent.isStopped = true;
        FacePlayer();
    }

    private void StartShooting()
    {
        isAiming = true;
        isShooting = true;
        isWalking = false;

        animator.SetBool("isAiming", true);
        animator.SetBool("isShooting", true);
        animator.SetBool("isWalking", false);

        agent.isStopped = true;
        FacePlayer();
    }

    private void StartWalkingToLastKnownPosition()
    {
        isAiming = false;
        isShooting = false;
        isWalking = true;

        animator.SetBool("isAiming", false);
        animator.SetBool("isShooting", false);
        animator.SetBool("isWalking", true);

        agent.isStopped = false;
        agent.SetDestination(lastKnownPlayerPosition);
    }

    private void GoIdle()
    {
        isAiming = false;
        isShooting = false;
        isWalking = false;

        animator.SetBool("isAiming", false);
        animator.SetBool("isShooting", false);
        animator.SetBool("isWalking", false);

        agent.isStopped = true;
    }

    private void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
}
