using UnityEngine;

public class PlayOnceAnimation : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            // Trigger the animation
            animator.SetTrigger("PlayOnce"); 

            // Optionally, disable the script after the animation finishes
            // This prevents the trigger from being set repeatedly
            animator.Play("notificationwrapper", -1, 0f); // Play the animation once
            Destroy(this); 
        }
    }
}