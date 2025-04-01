using UnityEngine;
using System.Reflection;

public class newtravelscript : MonoBehaviour
{
    [Header("Settings")]
    public GameObject targetObject;        // The object whose functions will be called
    public string openFunctionName;       // Function to call when entering the distance
    public string closeFunctionName;      // Function to call when exiting the distance
    public float triggerDistance = 5f;    // Distance to trigger the action

    private Transform playerTransform;
    private bool isWithinRange = false;   // Tracks if the player is currently within range

    private void Start()
    {
        // Find the player object by tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player not found! Ensure the player is tagged 'Player'.");
        }

        // Log initial state
        Debug.Log("ProximityTrigger initialized.");
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("Player transform is not assigned.");
            return;
        }

        // Calculate the distance between the player and this GameObject
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // Check if the player is within the trigger distance
        if (distance <= triggerDistance && !isWithinRange)
        {
            isWithinRange = true;
            Debug.Log("Player entered the range.");
            ExecuteFunction(openFunctionName);
        }
        else if (distance > triggerDistance && isWithinRange)
        {
            isWithinRange = false;
            Debug.Log("Player exited the range.");
            ExecuteFunction(closeFunctionName);
        }
    }

    private void ExecuteFunction(string functionName)
    {
        if (targetObject == null)
        {
            Debug.LogError("Target object is not assigned!");
            return;
        }

        if (string.IsNullOrEmpty(functionName))
        {
            Debug.LogError("Function name is not set.");
            return;
        }

        // Use reflection to find and invoke the method
        MonoBehaviour targetScript = targetObject.GetComponent<MonoBehaviour>();
        if (targetScript == null)
        {
            Debug.LogError($"No MonoBehaviour found on target object '{targetObject.name}'.");
            return;
        }

        MethodInfo method = targetScript.GetType().GetMethod(functionName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (method != null)
        {
            Debug.Log($"Executing function '{functionName}' on '{targetObject.name}'.");
            method.Invoke(targetScript, null);
        }
        else
        {
            Debug.LogError($"Function '{functionName}' not found on '{targetObject.name}'.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the trigger distance in the Scene view
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}