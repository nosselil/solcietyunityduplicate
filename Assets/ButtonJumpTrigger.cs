using UnityEngine;
using UnityEngine.Events;

public class ButtonTrigger : MonoBehaviour
{
    private bool isPlayerOnButton = false; // Track if the player is on the button
    private bool canPress = true; // Cooldown flag

    private Animator animator;

    // UnityEvent to call when the button is pressed
    public UnityEvent onButtonPressed;

    private void Start()
    {
        // Get the Animator component from the parent
        animator = GetComponentInParent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on the parent button!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object has the "Player" tag
        if (other.CompareTag("Jumper") && canPress)
        {
            Debug.Log("Player entered the button trigger.");
            isPlayerOnButton = true;
            TriggerButtonAnimation();
            canPress = false; // Disable further presses
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the colliding object has the "Player" tag
        if (other.CompareTag("Jumper"))
        {
            Debug.Log("Player left the button trigger.");
            isPlayerOnButton = false;
            canPress = true; // Re-enable button press
        }
    }

    private void TriggerButtonAnimation()
    {
        if (animator != null)
        {
            Debug.Log("Triggering button animation.");
            animator.SetTrigger("Press");

            // Invoke the UnityEvent
            onButtonPressed.Invoke();
        }
        else
        {
            Debug.LogError("Animator component not found!");
        }
    }
}