using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class CursorToggle : MonoBehaviour
{
    public Texture2D cursorTexture;
    public Vector2 hotspot = Vector2.zero;
    public CursorMode cursorMode = CursorMode.Auto;

    private bool isCursorLocked = true;
    public KeyCode toggleLockKey = KeyCode.C;
    public bool lockCursorAtStart = true;

    public GameObject dialogueObject;
    private bool dialogueActive = false;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        Debug.Log("CursorToggle: Start called.");

        SetCustomCursor();

        if (lockCursorAtStart)
        {
            LockCursor();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Update()
    {
        if (dialogueObject != null)
        {
            dialogueActive = dialogueObject.activeSelf;
        }
        else
        {
            dialogueActive = false;
        }

        if (!dialogueActive)
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                if (!isCursorLocked)
                {
                    Debug.Log("CursorToggle: Locking cursor on mouse click.");
                    LockCursor();
                }
            }
        }
        else
        {
            Debug.Log("CursorToggle: Dialogue active, unlocking cursor.");
            UnlockCursor();
        }

        if (Input.GetKeyDown(toggleLockKey))
        {
            Debug.Log("CursorToggle: Toggling cursor lock.");
            ToggleCursorLock();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("CursorToggle: New scene loaded, reapplying custom cursor.");
        SetCustomCursor();
    }

    void SetCustomCursor()
    {
        if (cursorTexture != null)
        {
            Debug.Log("CursorToggle: Setting custom cursor.");
            Cursor.SetCursor(cursorTexture, hotspot, cursorMode);
            Cursor.visible = true;
        }
        else
        {
            Debug.LogWarning("CursorToggle: Cursor texture not assigned.");
        }
    }

    void LockCursor()
    {
        Debug.Log("CursorToggle: Locking cursor.");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isCursorLocked = true;
        NetworkingDataContainer.Instance.allowPlayerCameraControlling = true;
    }

    void UnlockCursor()
    {
        Debug.Log("CursorToggle: Unlocking cursor.");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isCursorLocked = false;
        NetworkingDataContainer.Instance.allowPlayerCameraControlling = false;
    }

    void ToggleCursorLock()
    {
        if (isCursorLocked)
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}