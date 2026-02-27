using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class JumpScare : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioSource screamSource;
    
    [Header("Settings")]
    [SerializeField] private AudioClip scareSound;
    [SerializeField] private float scareDistance = 2.5f;

    [Header("UI")]
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeDuration = 0.5f;
    
    private Transform player;
    private NavMeshAgent agent;
    private bool hasScared = false;
    private PlayerMovement playerMovementScript;

    private void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) 
        {
            player = playerObj.transform;
            playerMovementScript = playerObj.GetComponent<PlayerMovement>();
        }

        agent = GetComponent<NavMeshAgent>();
        
        // Hide the panel at the start
        if (fadePanel != null)
        {
            Color c = fadePanel.color;
            c.a = 0;
            fadePanel.color = c;
        }
    }

    private void Update()
    {
        if (hasScared || player == null || agent == null) return;

        if (Vector3.Distance(transform.position, player.position) < scareDistance)
        {
            PerformScare();
        }
    }

    private void PerformScare()
    {
        hasScared = true;
        
        // 1. Stop the enemy
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        
        // 2. Freeze the player
        if (playerMovementScript != null)
        {
            playerMovementScript.DisableMovement();
        }
        
        // 3. Audio
        if (footstepSource != null) footstepSource.Stop();
        if (screamSource != null && scareSound != null)
        {
            screamSource.PlayOneShot(scareSound);
        }

        // 4. Start the Fade
        if (fadePanel != null)
        {
            StartCoroutine(FadeToBlack());
        }
    }

    private IEnumerator FadeToBlack()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            Color c = fadePanel.color;
            c.a = Mathf.Clamp01(elapsed / fadeDuration);
            fadePanel.color = c;
            yield return null;
        }
    }
}