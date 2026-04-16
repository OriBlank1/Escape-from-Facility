using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class BodyguardAI : MonoBehaviour
{
    public enum State { Guard, Chase, ReturnToPost, KnockedDown }
    public State currentState = State.Guard;

    [Header("References")]
    public Transform player;
    private NavMeshAgent agent;
    private Animator animator;

    [Header("AI Settings")]
    public float sightRange = 15f;
    public float fieldOfView = 120f;
    public float walkSpeed = 2f;
    public float runSpeed = 5f;

    [Header("Knockdown Settings")]
    public float knockdownDuration = 5f;
    public string holdableTag = "Holdable";

    // Variables to remember where the bodyguard started
    private Vector3 guardPostPosition;
    private Quaternion guardPostRotation;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        // Save the exact spot and rotation they started at
        guardPostPosition = transform.position;
        guardPostRotation = transform.rotation;

        agent.speed = walkSpeed;
        currentState = State.Guard;
    }

    void Update()
    {
        if (currentState == State.KnockedDown) return;

        UpdateAnimator();

        bool canSeePlayer = CanSeePlayer();

        // --- TEMPORARY DEBUG LINE ---
        if (canSeePlayer) { Debug.Log("I SEE THE PLAYER!"); }
        // ----------------------------

        if (canSeePlayer)
        {
            currentState = State.Chase;
        }
        else if (!canSeePlayer && currentState == State.Chase)
        {
            currentState = State.ReturnToPost;
        }

        HandleStateLogic();
    }

    private void HandleStateLogic()
    {
        switch (currentState)
        {
            case State.Guard:
                agent.isStopped = true;

                // Slowly rotate back to the original facing direction while guarding
                transform.rotation = Quaternion.Slerp(transform.rotation, guardPostRotation, Time.deltaTime * 5f);
                break;

            case State.Chase:
                agent.isStopped = false;
                agent.speed = runSpeed;
                agent.SetDestination(player.position);
                break;

            case State.ReturnToPost:
                agent.isStopped = false;
                agent.speed = walkSpeed;
                agent.SetDestination(guardPostPosition);

                // If we are close enough to the post, go back to Guard state
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    currentState = State.Guard;
                }
                break;
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
                return true;
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
        Debug.Log("collision hit");
        if (currentState == State.KnockedDown) return;

        // Note: 'other.gameObject' instead of 'collision.gameObject'
        if (collision.gameObject.CompareTag(holdableTag))
        {
            StartCoroutine(KnockdownRoutine());
        }
    }

    private IEnumerator KnockdownRoutine()
    {
        currentState = State.KnockedDown;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        animator.SetTrigger("hit");
        yield return new WaitForSeconds(1.5f); 

        // Once they get up, decide what to do next based on if the player is still there
        if (CanSeePlayer())
        {
            currentState = State.Chase;
        }
        else
        {
            currentState = State.ReturnToPost;
        }
    }
}