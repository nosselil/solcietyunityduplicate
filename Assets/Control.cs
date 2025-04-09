using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Control : MonoBehaviour
{
    public string nextSceneName = "startingarea"; // NOT IN USE

  
    private void Start()
    {
       
    }

    public void NextScene()
    {
        Debug.Log("TUTORIAL COMPLETED (NEXT SCENE): " + WalletManager.instance.tutorialCompleted);

        if (WalletManager.instance.tutorialCompleted)
            SceneManager.LoadScene("mainHub");
        else
            SceneManager.LoadScene("mainStartingarea");
    }
   

}