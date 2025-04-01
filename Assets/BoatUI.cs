using UnityEngine;

public class BoatUI : MonoBehaviour
{
    // Reference to the UI element prefab
    public GameObject uiElementPrefab;

    // Public variable to control the direction of movement
    public bool moveLeftToRight = true;

    // Speed of the movement
    public float moveSpeed = 100f;

    // Public variable to control the time before destruction
    public float destroyAfterSeconds = 5f;

    // Reference to the instantiated UI element
    private GameObject uiElementInstance;

    // Starting positions based on direction
    private Vector3 leftToRightStart = new Vector3(-145.083267f, -0.166625977f, 0f);
    private Vector3 rightToLeftStart = new Vector3(145.083313f, 0.166625977f, 0f);

    // This method is called when the GameObject is initialized
    void Start()
    {
        // Check if the prefab is assigned
        if (uiElementPrefab != null)
        {
            // Instantiate the UI element prefab
            uiElementInstance = Instantiate(uiElementPrefab);

            // Set the parent of the instantiated UI element to a Canvas
            uiElementInstance.transform.SetParent(GameObject.Find("Boats").transform, false);

            // Set the starting position based on the direction
            if (moveLeftToRight)
            {
                uiElementInstance.transform.localPosition = leftToRightStart;
            }
            else
            {
                uiElementInstance.transform.localPosition = rightToLeftStart;
            }

            // Destroy the UI element after the specified delay
            Destroy(uiElementInstance, destroyAfterSeconds);
        }
        else
        {
            Debug.LogError("UI Element Prefab is not assigned!");
        }
    }

    // This method is called once per frame
    void Update()
    {
        // Check if the UI element instance exists
        if (uiElementInstance != null)
        {
            // Calculate the movement direction
            float direction = moveLeftToRight ? 1f : -1f;

            // Move the UI element
            uiElementInstance.transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime);
        }
    }

    // This method is called when the GameObject is being destroyed
    private void OnDestroy()
    {
        // Check if the UI element instance exists
        if (uiElementInstance != null)
        {
            // Destroy the UI element
            Destroy(uiElementInstance);
            Debug.Log("UI element destroyed because parent GameObject is being destroyed.");
        }
    }
}