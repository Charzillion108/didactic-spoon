using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private enum EnemyState { Wandering, Chasing, Searching }

    [Header("Detection Settings")]
    public float detectionRange = 15f;
    public float fieldOfViewAngle = 110f; // Can't see behind itself
    public LayerMask obstructionMask;     // Set this to "Default" or "Environment"
    public Transform player;

    [Header("Movement Settings")]
    public float wanderRadius = 20f;
    public float wanderTimer = 5f;
    public float walkSpeed = 3f;
    public float chaseSpeed = 6.5f;

    [Header("Stalker Logic")]
    public float searchDuration = 5f; // How long it looks for you after losing sight
    
    private NavMeshAgent agent;
    private EnemyState currentState;
    private float timer;
    private float searchTimer;
    private Vector3 lastKnownPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentState = EnemyState.Wandering;
        
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Wandering:
                WanderLogic();
                if (CanSeePlayer()) StartChase();
                break;

            case EnemyState.Chasing:
                ChaseLogic();
                if (!CanSeePlayer()) StartSearching();
                break;

            case EnemyState.Searching:
                SearchLogic();
                if (CanSeePlayer()) StartChase();
                break;
        }
    }

    bool CanSeePlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > detectionRange) return false;

        // Check if player is within the viewing cone
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angleBetweenEnemyAndPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (angleBetweenEnemyAndPlayer < fieldOfViewAngle / 2f)
        {
            // Line of Sight check: Raycast to see if a wall is in the way
            if (!Physics.Linecast(transform.position + Vector3.up, player.position + Vector3.up, obstructionMask))
            {
                return true;
            }
        }
        return false;
    }

    void StartChase()
    {
        currentState = EnemyState.Chasing;
        agent.speed = chaseSpeed;
        // TIP: Trigger your "Mouth Opening" or "Eye Glow" animation here
        Debug.Log("Spotted! Starting Chase.");
    }

    void ChaseLogic()
    {
        agent.SetDestination(player.position);
        lastKnownPosition = player.position;
    }

    void StartSearching()
    {
        currentState = EnemyState.Searching;
        searchTimer = 0f;
        agent.SetDestination(lastKnownPosition); // Go to where we last saw them
        Debug.Log("Lost sight. Searching...");
    }

    void SearchLogic()
    {
        searchTimer += Time.deltaTime;
        
        // If it reaches the last known spot, it "looks around" (small wander)
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            timer += Time.deltaTime;
            if (timer >= 2f) // Every 2 seconds pick a spot nearby
            {
                agent.SetDestination(RandomNavSphere(transform.position, 5f, -1));
                timer = 0;
            }
        }

        if (searchTimer >= searchDuration)
        {
            currentState = EnemyState.Wandering;
            agent.speed = walkSpeed;
        }
    }

    void WanderLogic()
    {
        timer += Time.deltaTime;
        if (timer >= wanderTimer)
        {
            agent.SetDestination(RandomNavSphere(transform.position, wanderRadius, -1));
            timer = 0;
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizes the FOV in the Editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Vector3 fovLine1 = Quaternion.AngleAxis(fieldOfViewAngle / 2, transform.up) * transform.forward * detectionRange;
        Vector3 fovLine2 = Quaternion.AngleAxis(-fieldOfViewAngle / 2, transform.up) * transform.forward * detectionRange;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, fovLine1);
        Gizmos.DrawRay(transform.position, fovLine2);
    }
}