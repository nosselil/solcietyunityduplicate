using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Control : MonoBehaviour
{
    public string nextSceneName = "startingarea";


  

    private void Start()
    {
       
    }

    public void NextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
   

}