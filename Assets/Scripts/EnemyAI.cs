using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField] private Transform player;
    private NavMeshAgent agent;

    [Header("Behavior")]
    [SerializeField] private float chaseSpeed = 8f;
    [SerializeField] private float stopDistance = 1.5f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = chaseSpeed;
        agent.stoppingDistance = stopDistance;
        
        // Auto-find Player if not assigned
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (player != null)
        {
            // The enemy always knows where the player is in the dark
            agent.SetDestination(player.position);
        }
    }
}