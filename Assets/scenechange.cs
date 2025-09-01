using UnityEngine;
using UnityEngine.SceneManagement;

public class Control : MonoBehaviour
{
    public string nextSceneName = "startingarea"; 

    public void NextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}