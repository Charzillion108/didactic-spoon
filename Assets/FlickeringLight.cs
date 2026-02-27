using UnityEngine;
using System.Collections;

public class FlickeringLight : MonoBehaviour
{
    [SerializeField] private Light lightComponent;
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 1.5f;
    [SerializeField] private float timeBetweenFlickers = 0.05f;

    private void Awake()
    {
        if (lightComponent == null)
            lightComponent = GetComponent<Light>();
        
        StartCoroutine(Flicker());
    }

    private IEnumerator Flicker()
    {
        while (true)
        {
            // Set a random intensity
            lightComponent.intensity = Random.Range(minIntensity, maxIntensity);
            
            // Wait for a small, random amount of time before the next flicker
            yield return new WaitForSeconds(Random.Range(0, timeBetweenFlickers));
        }
    }
}