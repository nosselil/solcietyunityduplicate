using UnityEngine;

public class SelfDestructAfterDelay : MonoBehaviour
{
    public float delay = 3.0f; // Time in seconds before self-destruction

    void Start()
    {
        // Call the Destroy method with a delay
        Destroy(gameObject, delay);
    }
}