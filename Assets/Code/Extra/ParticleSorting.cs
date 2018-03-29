using UnityEngine;

public class ParticleSorting : MonoBehaviour
{
    Renderer particleSystemRenderer;
    
    public int sortingOrder = 0;

    void Update()
    {
        particleSystemRenderer.sortingOrder = sortingOrder;
    }
    void Awake()
    {
        particleSystemRenderer = GetComponent<Renderer>();

        // Set the sorting layer of the particle system.
    }
}