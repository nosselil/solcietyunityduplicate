using UnityEngine;

public class MoveDownward : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveDistance = 50f; // Distance to move downward
    public float duration = 2f;    // Time in seconds to complete the movement
    public float delay = 0.5f;     // Delay in seconds before movement starts
    public bool moveOnStart = true; // Whether to move automatically on start

    private void Start()
    {
        if (moveOnStart)
        {
            MoveDown();
        }
    }

    public void MoveDown()
    {
        // Get the target position by subtracting the moveDistance from the current Y position
        Vector3 targetPosition = transform.position - new Vector3(0, moveDistance, 0);

        // Use LeanTween to move the GameObject to the target position
        LeanTween.move(gameObject, targetPosition, duration)
            .setDelay(delay) // Add a delay before the movement starts
            .setEase(LeanTweenType.easeOutQuad); // Optional: Set an easing type for smooth movement
    }
}