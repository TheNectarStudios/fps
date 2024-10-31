using UnityEngine;
using UnityEngine.AI;

public class EnemyAIController : MonoBehaviour
{
    public Transform player;  // Reference to the player Transform
    public float detectionRange = 20f;  // Detection range for the enemy
    public float aimRange = 15f;  // Range to start aiming at the player
    public float shootingRange = 10f;  // Range to start shooting at the player
    public float fieldOfView = 60f;  // Field of view for detecting the player
    public float rotationSpeed = 5f;  // Speed at which the enemy rotates towards the player

    private Animator animator;  // Reference to the Animator component
    private NavMeshAgent agent;  // Reference to the NavMeshAgent component
    private bool isAiming = false;  // State for aiming
    private bool isShooting = false;  // State for shooting
    private bool isWalking = false;  // State for walking

    private Vector3 lastKnownPlayerPosition;  // Last known position of the player
    private bool playerInSight = false;  // Check if the player is in sight

    void Start()
    {
        animator = GetComponent<Animator>();  // Get the Animator component
        agent = GetComponent<NavMeshAgent>();  // Get the NavMeshAgent component
        agent.updateRotation = false;  // Disable agent's auto-rotation
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);  // Calculate distance to the player

        // Check if the player is within detection range and field of view
        playerInSight = IsPlayerInSight();

        // Update Animator parameters based on conditions
        animator.SetBool("isPlayerNear", distanceToPlayer <= detectionRange);
        animator.SetBool("canSeePlayer", playerInSight);
        
        // Control enemy behavior based on player position
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

        // Face the player while aiming or shooting
        if (isAiming || isShooting)
        {
            FacePlayer();
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
        Vector3 directionToPlayer = (player.position - transform.position).normalized;  // Direction towards the player
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);  // Angle to the player

        // Check if player is within the field of view
        if (angleToPlayer < fieldOfView / 2)
        {
            Ray ray = new Ray(transform.position, directionToPlayer);  // Create a ray to check for line of sight
            if (Physics.Raycast(ray, out RaycastHit hit, detectionRange))  // Perform the raycast
            {
                if (hit.transform == player)  // Check if the hit object is the player
                {
                    lastKnownPlayerPosition = player.position;  // Update last known player position
                    return true;  // Player is in sight
                }
            }
        }
        return false;  // Player is not in sight
    }

    private void StartAiming()
    {
        isAiming = true;
        isShooting = false;
        isWalking = false;

        animator.SetBool("isAiming", true);
        animator.SetBool("isShooting", false);
        animator.SetBool("isWalking", false);

        agent.isStopped = true;  // Stop movement while aiming
    }

    private void StartShooting()
    {
        isAiming = true;
        isShooting = true;
        isWalking = false;

        animator.SetBool("isAiming", true);
        animator.SetBool("isShooting", true);
        animator.SetBool("isWalking", false);

        agent.isStopped = true;  // Stop movement while shooting
    }

    private void StartWalkingToLastKnownPosition()
    {
        isAiming = false;
        isShooting = false;
        isWalking = true;

        animator.SetBool("isAiming", false);
        animator.SetBool("isShooting", false);
        animator.SetBool("isWalking", true);

        agent.isStopped = false;  // Allow movement
        agent.SetDestination(lastKnownPlayerPosition);  // Move to last known player position
    }

    private void GoIdle()
    {
        isAiming = false;
        isShooting = false;
        isWalking = false;

        animator.SetBool("isAiming", false);
        animator.SetBool("isShooting", false);
        animator.SetBool("isWalking", false);

        agent.isStopped = true;  // Stop movement in idle state
    }

    private void FacePlayer()
    {
        if (playerInSight)  // Only rotate if the player is in sight
        {
            Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, player.position.z);  // Ignore vertical difference
            transform.LookAt(targetPosition);  // Instantly face the player

            // Debugging line
            Debug.Log("Facing player at position: " + player.position);
        }
    }
}
