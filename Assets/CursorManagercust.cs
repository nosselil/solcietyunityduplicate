using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTexture; // Assign your custom cursor texture in the Inspector
    [SerializeField] private Vector2 hotSpot = Vector2.zero; // Set the hotspot (pivot) of the cursor

    private void Awake()
    {
        // Ensure this GameObject persists across scenes
        DontDestroyOnLoad(gameObject);

        // Set the custom cursor
        SetCustomCursor();
    }

    private void Update()
    {
        // Continuously enforce the custom cursor
        SetCustomCursor();
    }

    private void SetCustomCursor()
    {
        // Apply the custom cursor texture
        Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
    }
}