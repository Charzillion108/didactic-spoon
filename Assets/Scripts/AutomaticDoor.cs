using UnityEngine;
using System.Collections;

public class AutomaticDoor : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag the actual Door Model/Cube here")]
    [SerializeField] private Transform doorMesh; 

    [Header("Movement Settings")]
    [SerializeField] private Vector3 openOffset = new Vector3(0, 3f, 0);
    [SerializeField] private float duration = 1.0f;
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Audio")]
    [SerializeField] private AudioSource doorAudioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Coroutine movementCoroutine;

    private void Start()
    {
        if (doorMesh == null)
        {
            Debug.LogError("Door Mesh is missing! Drag your door into the slot on " + gameObject.name);
            return;
        }
        
        closedPosition = doorMesh.position;
        openPosition = closedPosition + openOffset;
    }

    private void OnTriggerEnter(Collider other)
    {
        // DEBUG: This will tell us if the player is even being detected
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered door trigger!");
            PlaySound(openSound);
            MoveDoor(openPosition);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player left door trigger!");
            PlaySound(closeSound);
            MoveDoor(closedPosition);
        }
    }

    private void MoveDoor(Vector3 target)
    {
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        movementCoroutine = StartCoroutine(AnimateDoor(target));
    }

    private IEnumerator AnimateDoor(Vector3 target)
    {
        Vector3 startPos = doorMesh.position;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percentage = elapsed / duration;
            doorMesh.position = Vector3.Lerp(startPos, target, movementCurve.Evaluate(percentage));
            yield return null;
        }

        doorMesh.position = target;
    }

    private void PlaySound(AudioClip clip)
    {
        if (doorAudioSource != null && clip != null)
        {
            doorAudioSource.PlayOneShot(clip);
        }
    }
}