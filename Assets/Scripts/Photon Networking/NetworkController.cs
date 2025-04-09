using UnityEngine;
using static Unity.Collections.Unicode;
using UnityEngine.SceneManagement;
using Fusion;
using System.Threading.Tasks;


public class NetworkController : NetworkBehaviour
{
    public static NetworkController Instance;
    public bool loadingNewScene = false;

    [HideInInspector] public bool localPlayerSpawned = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        loadingNewScene = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    /*public void SwitchScene(string newSceneName)
    {
        //await SwitchRoomAndScene(newSceneName);
    }*/

    public async void SwitchRoomAndScene(string newSceneName)
    {
        if (WalletManager.instance)
            Debug.Log("TUTORIAL: New scene name is " +  newSceneName + ", wallet manager tut completed: " + WalletManager.instance.tutorialCompleted);        

        if (!WalletManager.instance.tutorialCompleted && newSceneName == "MainHub")
        {
            Debug.Log("TUTORIAL COMPLETED (SWITCH ROOM AND SCENE IN MAIN HUB): " + WalletManager.instance.tutorialCompleted);
            WalletManager.instance.tutorialCompleted = true;
            PlayerPrefs.SetInt(WalletManager.instance.PLAYER_PREFS_TUTORIAL_COMPLETED_KEY, 1); // mark tutorial as completed
        }

        loadingNewScene = true; // A flag that will prevent the player from interacting or moving // TODO: Does not currently do much

        // Disable interactions when new scene is loading
        Runner.GetPlayerObject(Runner.LocalPlayer).GetComponentInChildren<PixelCrushers.DialogueSystem.ProximitySelector>().enabled = false;

        //Debug.Break();

        // This will disconnect from the current session and clean up networked objects.
        if (Runner != null)
        {
            await Runner.Shutdown();
        }        

        // Now load the new scene.
        SceneManager.LoadScene(newSceneName);        
    }
}

