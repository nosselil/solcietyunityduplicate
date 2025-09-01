using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[System.Serializable]
public class MapLocation
{
    public string id;           // Unique ID for the location (e.g., "gallery", "defi_district")
    public string sceneName;    // Scene name (e.g., "Gallery", "MainHub")
    public string markerName;   // Name of the marker GameObject in the scene
}

public class MapTeleportManager : MonoBehaviour
{
    [Header("List of teleport locations")]
    public List<MapLocation> locations = new List<MapLocation>();

    [Header("Player reference (optional, will auto-find if left empty)")]
    public GameObject player;

    public TextMeshProUGUI statusText;

    public void TeleportToLocation(string locationId)
    {
        MapLocation loc = locations.Find(l => l.id == locationId);
        if (loc == null)
        {
            Debug.LogWarning("No location found with ID: " + locationId);
            return;
        }

        // Find player if not assigned
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("Player object not found! Assign it in the inspector or tag your player as 'Player'.");
                return;
            }
        }

        if (SceneManager.GetActiveScene().name == loc.sceneName)
        {
            TeleportPlayer(loc);
        }
        else
        {
            StartCoroutine(LoadSceneAndTeleport(loc));
        }
    }

    void TeleportPlayer(MapLocation loc)
    {
        Debug.Log("TeleportPlayer called for marker: " + loc.markerName);
        GameObject markerObj = GameObject.Find(loc.markerName);
        if (markerObj == null)
        {
            Debug.LogError("Marker not found: " + loc.markerName);
            return;
        }
        Debug.Log("Teleporting player to: " + markerObj.transform.position);

        // Disable movement scripts
        var walker = player.GetComponent("AdvancedWalkerController");
        var mover = player.GetComponent("Mover");
        var kcc = player.GetComponent("SimpleKCC");
        var keyboardInput = player.GetComponent("CharacterKeyboardInput");
        if (walker != null) ((MonoBehaviour)walker).enabled = false;
        if (mover != null) ((MonoBehaviour)mover).enabled = false;
        if (kcc != null) ((MonoBehaviour)kcc).enabled = false;
        if (keyboardInput != null) ((MonoBehaviour)keyboardInput).enabled = false;

        // Teleport
        StartCoroutine(ForceTeleport(player, markerObj.transform.position, markerObj.transform.rotation));

        // Re-enable movement scripts
        if (walker != null) ((MonoBehaviour)walker).enabled = true;
        if (mover != null) ((MonoBehaviour)mover).enabled = true;
        if (kcc != null) ((MonoBehaviour)kcc).enabled = true;
        if (keyboardInput != null) ((MonoBehaviour)keyboardInput).enabled = true;
    }

    IEnumerator LoadSceneAndTeleport(MapLocation loc)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(loc.sceneName);
        while (!asyncLoad.isDone)
            yield return null;

        // Wait one frame for objects to initialize
        yield return null;

        // Find player again in the new scene
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player object not found in new scene!");
            yield break;
        }

        // Find the marker in the new scene by name
        GameObject markerObj = GameObject.Find(loc.markerName);
        if (markerObj == null)
        {
            Debug.LogError("Marker not found: " + loc.markerName);
            yield break;
        }

        player.transform.position = markerObj.transform.position;
        player.transform.rotation = markerObj.transform.rotation;
    }

    IEnumerator ForceTeleport(GameObject player, Vector3 pos, Quaternion rot)
    {
        yield return new WaitForFixedUpdate();
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.position = pos;
            rb.rotation = rot;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
        }
        else
        {
            player.transform.position = pos;
            player.transform.rotation = rot;
        }
    }

    void UpdateSceneStatus()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName == "MainHub")
        {
            statusText.text = "MainHub";
        }
        else if (sceneName == "mainGalleryMultiplayer" || sceneName == "Gallery")
        {
            statusText.text = "Gallery";
        }
        else
        {
            statusText.text = $"{sceneName}";
        }
    }

    void Start()
    {
        UpdateSceneStatus();
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    void OnSceneChanged(UnityEngine.SceneManagement.Scene oldScene, UnityEngine.SceneManagement.Scene newScene)
    {
        UpdateSceneStatus();
    }
}