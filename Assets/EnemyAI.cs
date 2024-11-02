using UnityEngine;
using UnityEngine.AI;

public class EnemyAIController : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 20f;
    public float shootingRange = 10f;
    public float fieldOfView = 60f;
    public float rotationSpeed = 5f;
    public float rotationOffset = 60f; // Offset angle for aiming and shooting

    private Animator animator;
    private NavMeshAgent agent;

    private Vector3 lastKnownPlayerPosition;
    private bool playerInSight = false;
    private bool playerWasSpotted = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        playerInSight = IsPlayerInSight();

        if (playerInSight)
        {
            // Player spotted, remember their position and stop moving
            lastKnownPlayerPosition = player.position;
            playerWasSpotted = true;
            agent.isStopped = true;

            if (Vector3.Distance(transform.position, player.position) <= shootingRange)
            {
                StartShooting();
            }
            else
            {
                StartAiming();
            }
        }
        else if (playerWasSpotted)
        {
            // Player out of sight, move to last known position if not already there
            MoveToLastKnownPosition();
        }
        else
        {
            // No knowledge of player’s position, idle
            GoIdle();
        }

        if (animator.GetBool("isAiming") || animator.GetBool("isShooting"))
        {
            FacePlayerSmoothlyWithOffset();
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
                    return true;
                }
            }
        }
        return false;
    }

    private void StartAiming()
    {
        animator.SetBool("isAiming", true);
        animator.SetBool("isShooting", false);
        animator.SetBool("isWalking", false);

        agent.isStopped = true;  // Stop moving to aim
    }

    private void StartShooting()
    {
        animator.SetBool("isAiming", true);
        animator.SetBool("isShooting", true);
        animator.SetBool("isWalking", false);

        agent.isStopped = true;  // Stop moving to shoot
    }

    private void MoveToLastKnownPosition()
    {
        if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 1f)
        {
            // Reached last known position, stop moving
            playerWasSpotted = false;
            GoIdle();
            return;
        }

        animator.SetBool("isAiming", false);
        animator.SetBool("isShooting", false);
        animator.SetBool("isWalking", true);

        agent.isStopped = false;
        agent.SetDestination(lastKnownPlayerPosition);
    }

    private void GoIdle()
    {
        animator.SetBool("isAiming", false);
        animator.SetBool("isShooting", false);
        animator.SetBool("isWalking", false);

        agent.isStopped = true;  // Idle, stop moving
    }

    private void FacePlayerSmoothlyWithOffset()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));

        // Apply the 60-degree offset to the look rotation
        Quaternion offsetRotation = Quaternion.Euler(0, rotationOffset, 0) * lookRotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, offsetRotation, Time.deltaTime * rotationSpeed);
    }
}
