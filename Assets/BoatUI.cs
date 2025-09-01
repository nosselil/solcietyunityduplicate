using UnityEngine;

public class BoatUI : MonoBehaviour
{
    // Reference to the UI element prefab
    public GameObject uiElementPrefab;

    // Public variable to control the direction of movement
    public bool moveLeftToRight = true;

    // Speed of the movement
    public float moveSpeed = 100f;

    // Reference to the instantiated UI element
    private GameObject uiElementInstance;

    // Starting positions based on direction
    private Vector3 leftToRightStart = new Vector3(-145.083267f, -0.166625977f, 0f);
    private Vector3 rightToLeftStart = new Vector3(145.083313f, 0.166625977f, 0f);

    // Reference to the real boat's transform
    public Transform boatTransform;

    private Canvas mainCanvas;

    public RectTransform lineStart;
    public RectTransform lineEnd;
    public float worldStartZ;       // The Z position in world space where the line starts
    public float worldEndZ;         // The Z position in world space where the line ends

    public Transform destinationTransform; // Drag your destination object here in the Inspector

    // This method is called when the GameObject is initialized
    void Start()
    {
        // Auto-assign if not set in Inspector
        if (lineStart == null)
        {
            GameObject startObj = GameObject.Find("purple");
            if (startObj != null)
                lineStart = startObj.GetComponent<RectTransform>();
            else
                Debug.LogError("Could not find UI element 'purple' for lineStart!");
        }

        if (lineEnd == null)
        {
            GameObject endObj = GameObject.Find("red");
            if (endObj != null)
                lineEnd = endObj.GetComponent<RectTransform>();
            else
                Debug.LogError("Could not find UI element 'red' for lineEnd!");
        }

        // Destination assignment based on direction
        if (moveLeftToRight)
        {
            GameObject enemyHealthObj = GameObject.Find("EnemyHealthbox");
            if (enemyHealthObj != null)
                destinationTransform = enemyHealthObj.transform;
            else
                Debug.LogError("Could not find destination object 'EnemyHealthbox'!");
        }
        else if (destinationTransform == null)
        {
            GameObject destObj = GameObject.Find("ArrowMarker");
            if (destObj != null)
                destinationTransform = destObj.transform;
            else
                Debug.LogError("Could not find destination object 'ArrowMarker'!");
        }

        if (uiElementPrefab != null)
        {
            uiElementInstance = Instantiate(uiElementPrefab);
            mainCanvas = GameObject.FindObjectOfType<Canvas>();
            uiElementInstance.transform.SetParent(mainCanvas.transform, false);

            // Set the starting position based on the direction
            if (moveLeftToRight)
            {
                uiElementInstance.transform.localPosition = leftToRightStart;
            }
            else
            {
                uiElementInstance.transform.localPosition = rightToLeftStart;
            }
        }
        else
        {
            Debug.LogError("UI Element Prefab is not assigned!");
        }

        if (destinationTransform != null)
        {
            worldStartZ = transform.position.z;
            worldEndZ = destinationTransform.position.z;
        }
    }

    // This method is called once per frame
    void Update()
    {
        if (uiElementInstance != null && boatTransform != null && lineStart != null && lineEnd != null)
        {
            float t = Mathf.InverseLerp(worldStartZ, worldEndZ, boatTransform.position.z);
            Vector3 uiPos = Vector3.Lerp(lineStart.position, lineEnd.position, moveLeftToRight ? t : 1 - t);
            uiElementInstance.GetComponent<RectTransform>().position = uiPos;
        }

        // Destroy the boat if it reaches the destination (for left to right)
        if (moveLeftToRight && destinationTransform != null)
        {
            float distanceToDest = Mathf.Abs(transform.position.z - destinationTransform.position.z);
            if (distanceToDest < 0.1f) // You can adjust the threshold as needed
            {
                Destroy(gameObject);
            }
        }
    }

    // This method is called when the GameObject is being destroyed
    private void OnDestroy()
    {
        if (uiElementInstance != null)
        {
            Destroy(uiElementInstance);
            Debug.Log("UI element destroyed because parent GameObject is being destroyed.");
        }
    }
}