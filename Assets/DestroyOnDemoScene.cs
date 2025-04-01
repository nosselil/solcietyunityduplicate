using UnityEngine;
using UnityEngine.SceneManagement;

public class DestroyOnDemoScene : MonoBehaviour
{
    private void OnEnable()
    {
        // Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from the sceneLoaded event to avoid memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if the loaded scene is named "Demo"
        if (scene.name == "Demo")
        {
            // Destroy the GameObject this script is attached to (and all its children)
            Destroy(gameObject);
        }
    }
}