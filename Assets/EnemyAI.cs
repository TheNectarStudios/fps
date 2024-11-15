using UnityEngine;
using UnityEngine.AI;

public class EnemyAIController : MonoBehaviour
{
    public Transform player;
    public Transform firePoint;
    public GameObject bulletPrefab;
    public AudioClip shootingSound;
    public float detectionRange = 20f;
    public float shootingRange = 10f;
    public float fieldOfView = 60f;
    public float rotationSpeed = 5f;
    public float fireRate = 1f;

    private Animator animator;
    private NavMeshAgent agent;
    private AudioSource audioSource;

    private Vector3 lastKnownPlayerPosition;
    private bool playerInSight = false;
    private bool playerWasSpotted = false;
    private float nextFireTime = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        playerInSight = IsPlayerInSight();

        if (playerInSight)
        {
            // Player spotted, remember their position
            lastKnownPlayerPosition = player.position;
            playerWasSpotted = true;

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
            // Player out of sight but previously spotted
            MoveToLastKnownPosition();
        }
        else
        {
            // No knowledge of player’s position, idle
            GoIdle();
        }

        if (animator.GetBool("isAiming") || animator.GetBool("isShooting"))
        {
            FacePlayerSmoothly();
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

        if (Time.time >= nextFireTime)
        {
            // Instantiate bullet at the fire point
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // Play the shooting sound
            audioSource.PlayOneShot(shootingSound);

            // Set the next fire time
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    private void MoveToLastKnownPosition()
    {
        animator.SetBool("isAiming", false);
        animator.SetBool("isShooting", false);
        animator.SetBool("isWalking", true);

        agent.isStopped = false;
        agent.SetDestination(lastKnownPlayerPosition);

        if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 1f)
        {
            playerWasSpotted = false;  // Reached last known position, stop moving
            GoIdle();
        }
    }

    private void GoIdle()
    {
        animator.SetBool("isAiming", false);
        animator.SetBool("isShooting", false);
        animator.SetBool("isWalking", false);

        agent.isStopped = true;  // Idle, stop moving
    }

    private void FacePlayerSmoothly()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
}
