using UnityEngine;
using UnityEngine.SceneManagement;

public class CombinedCursorManager : MonoBehaviour
{
    public Texture2D cursorTexture;
    public Vector2 hotspot = Vector2.zero;
    public CursorMode cursorMode = CursorMode.Auto;

    public bool lockCursorAtGameStart = true;
    public KeyCode unlockKeyCode = KeyCode.Escape;
    public KeyCode lockKeyCode = KeyCode.Mouse0;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        SetCustomCursor();

        if (lockCursorAtGameStart && cursorTexture == null)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetCustomCursor();
    }

    void Update()
    {
        if (Input.GetKeyDown(unlockKeyCode))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = cursorTexture != null; // Show cursor if custom cursor is active
        }

        if (Input.GetKeyDown(lockKeyCode))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false; // Hide cursor when locking
        }
    }

    void SetCustomCursor()
    {
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, hotspot, cursorMode);
            Cursor.visible = true;
        }
        else
        {
            Debug.LogWarning("Cursor texture not assigned in CursorManager.");
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}