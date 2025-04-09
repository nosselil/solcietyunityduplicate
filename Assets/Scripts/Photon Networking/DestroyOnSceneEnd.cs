using UnityEngine;
using UnityEngine.SceneManagement;

public class DestroyOnSceneUnload : MonoBehaviour
{
    // Store the scene that this GameObject originally belongs to.
    private Scene originalScene;

    void Start()
    {
        // Record the scene that this object is in at startup.
        originalScene = gameObject.scene;
    }

    void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    void OnSceneUnloaded(Scene unloadedScene)
    {
        // If the unloaded scene is the scene this object originally belonged to,
        // destroy this GameObject.
        //if (unloadedScene == originalScene)
        {
            Destroy(gameObject);
        }
    }
}
