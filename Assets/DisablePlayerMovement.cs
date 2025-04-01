using UnityEngine;

public class DisableComponents : MonoBehaviour
{
    // Public fields to drag and drop components in the Inspector
    public Component component1; // First component to disable
    public Component component2; // Second component to disable

    // Flag to track whether components are enabled or disabled
    private bool areComponentsEnabled = true;

    // Public method to toggle components on/off
    public void ToggleComponents()
    {
        // Toggle the state of the components
        areComponentsEnabled = !areComponentsEnabled;

        // Enable or disable the components based on the flag
        SetComponentsEnabled(areComponentsEnabled);

        // Log the current state
        Debug.Log("Components Enabled: " + areComponentsEnabled);
    }

    // Public method to enable components
    public void EnableComponents()
    {
        areComponentsEnabled = true;
        SetComponentsEnabled(true);
        Debug.Log("Components Enabled: " + areComponentsEnabled);
    }

    // Public method to disable components
    public void DisableComponentsMethod()
    {
        areComponentsEnabled = false;
        SetComponentsEnabled(false);
        Debug.Log("Components Enabled: " + areComponentsEnabled);
    }

    // Internal method to set the enabled state of the components
    private void SetComponentsEnabled(bool enabled)
    {
        // Disable or enable the first component
        if (component1 != null)
        {
            if (component1 is Behaviour) // Check if it's a Behaviour (e.g., MonoBehaviour)
            {
                ((Behaviour)component1).enabled = enabled;
            }
            else if (component1 is Collider) // Check if it's a Collider
            {
                ((Collider)component1).enabled = enabled;
            }
            else if (component1 is Renderer) // Check if it's a Renderer
            {
                ((Renderer)component1).enabled = enabled;
            }
            // Add more component types here if needed
        }

        // Disable or enable the second component
        if (component2 != null)
        {
            if (component2 is Behaviour) // Check if it's a Behaviour (e.g., MonoBehaviour)
            {
                ((Behaviour)component2).enabled = enabled;
            }
            else if (component2 is Collider) // Check if it's a Collider
            {
                ((Collider)component2).enabled = enabled;
            }
            else if (component2 is Renderer) // Check if it's a Renderer
            {
                ((Renderer)component2).enabled = enabled;
            }
            // Add more component types here if needed
        }
    }
}