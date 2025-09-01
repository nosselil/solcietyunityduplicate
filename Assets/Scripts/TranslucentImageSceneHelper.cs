using UnityEngine;
using UnityEngine.SceneManagement;
using LeTai.Asset.TranslucentImage;

public class TranslucentImageSceneHelper : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (var img in FindObjectsOfType<TranslucentImage>())
        {
            // Use reflection to call ForceAcquireSource if it exists
            var method = img.GetType().GetMethod("ForceAcquireSource", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (method != null)
                method.Invoke(img, null);
        }
    }
}