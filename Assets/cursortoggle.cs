using UnityEngine;
using UnityEngine.EventSystems;

public class CursorLockToggle : MonoBehaviour
{
    private bool isCursorLocked = true;

    void Start()
    {
        ToggleCursorLock();
        DisableCursor();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) 
        {
            if (!isCursorLocked) // Only lock if it's currently unlocked
            {
                DisableCursor();
                isCursorLocked = true;
            }
        }

        // Check for C key press
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleCursorLock(); 
        }
    }

    public void ToggleCursorLock()
    {
        if (isCursorLocked)
        {
            EnableCursor();
        }
        else
        {
            DisableCursor();
        }

        isCursorLocked = !isCursorLocked;
    }

    public void EnableCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void DisableCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}