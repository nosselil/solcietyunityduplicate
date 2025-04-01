using UnityEngine;

public class ButtonAnimator : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        // Get the Animator component
        animator = GetComponent<Animator>();
    }

    // Public method to trigger the button press animation
    public void AnimateButtonPress()
    {
        if (animator != null)
        {
            animator.SetTrigger("Press"); // Set the trigger
        }
        else
        {
            Debug.LogError("Animator component not found!");
        }
    }
}