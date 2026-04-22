using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class BodyguardAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    private NavMeshAgent agent;
    private Animator animator;

    [Header("AI Settings")]
    public float sightRange = 15f;
    public float fieldOfView = 120f;
    public float runSpeed = 5f;

    [Header("Knockdown Settings")]
    public string holdableTag = "Holdable";

    // A simple switch: once this flips to true, they never stop chasing.
    private bool hasSpottedPlayer = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Find player if not assigned in the Inspector
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        // Stand perfectly still when the level starts
        agent.speed = runSpeed;
        agent.isStopped = true;
    }

    void Update()
    {
        UpdateAnimator();

        // If they haven't seen you yet, keep checking
        if (!hasSpottedPlayer)
        {
            if (CanSeePlayer())
            {
                // The moment they see you, flip the switch to true!
                hasSpottedPlayer = true;
                agent.isStopped = false; // Release them so they can move
            }
        }
        else
        {
            // If they have spotted you, constantly update their destination to your position
            agent.SetDestination(player.position);
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < sightRange)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer < fieldOfView / 2f)
            {
                // Start laser at bodyguard's chest
                Vector3 eyePosition = transform.position + Vector3.up * 1.5f;

                // Shoot laser directly at the Main Camera (which is already at head-height)
                Vector3 targetPosition = player.position;

                Vector3 rayDirection = (targetPosition - eyePosition).normalized;

                Debug.DrawRay(eyePosition, rayDirection * sightRange, Color.red);

                if (Physics.Raycast(eyePosition, rayDirection, out RaycastHit hit, sightRange))
                {
                    Debug.Log("Bodyguard is looking at: " + hit.transform.name);

                    if (hit.transform == player || hit.transform.IsChildOf(player))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void UpdateAnimator()
    {
        // Update animator parameters based on agent speed
        float speedPercent = agent.velocity.magnitude / runSpeed;
        animator.SetFloat("Speed", speedPercent, 0.1f, Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collision hit!");

        // If hit by the crowbar, destroy the bodyguard immediately
        if (collision.gameObject.CompareTag(holdableTag))
        {
            Destroy(gameObject);
        }
    }
}