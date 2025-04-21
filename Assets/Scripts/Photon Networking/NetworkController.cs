using UnityEngine;
using static Unity.Collections.Unicode;
using UnityEngine.SceneManagement;
using Fusion;
using System.Threading.Tasks;
using System.Collections;
using Unity.VisualScripting;


public class NetworkController : NetworkBehaviour
{
    public static NetworkController Instance;
    //[HideInInspector] public bool loadingNewScene = false;

    [HideInInspector] public bool localPlayerSpawned = false;

    //[SerializeField] GameObject disconnectionPopUp;

    public int maxPlayersPerReplica = 32;

    //private Coroutine localPlayerExistencePollingCoroutine = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        //disconnectionPopUp.SetActive(false);
        //loadingNewScene = false;

        //localPlayerExistencePollingCoroutine = 
            //StartCoroutine(PollLocalPlayerExistence());
        //GetComponentInChildren<NetworkEvents>().OnShutdown.AddListener(OnRunnerShutdown);
    }

    /*IEnumerator PollLocalPlayerExistence()
    {
        float pollInterval = 3f;

        yield return new WaitForSeconds(5f); // Wait a moment for the player instantiation to happen properly

        while (true)
        {
            Debug.Log("DISCONNECT: Local player still exists");

            if (Runner != null)
            {
                if (Runner.State == NetworkRunner.States.Shutdown && Runner.GetPlayerObject(Runner.LocalPlayer) == null)
                {
                    // The local player doesn't exist anymore, so display disconnection pop-up (that will take us back to the main menu)
                    //DisplayDisconnectionPopUp();
                    Debug.Log("DISCONNECT: Local player doesn't exist any more");
                    yield break;
                }
                else if (!Runner.IsRunning || !Runner.IsConnectedToServer || Runner.CurrentConnectionType == ConnectionType.None)
                {
                    //DisplayDisconnectionPopUp();
                    Debug.Log("DISCONNECT: Runner is not running, connected to server or the connection type is none");
                    yield break;
                }
            }

            yield return new WaitForSeconds(pollInterval);            
        }
    }*/

    /*public void DisplayDisconnectionPopUp()
    {
        //disconnectionPopUp.SetActive(true);
    }*/

    
    /*public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }*/

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("DISCONNECT: Network controller still exists");
        }
    }

    public void OnRunnerShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("SHUTDOWN: Shutdown triggered. Reason: " + shutdownReason);
        // Your logic to handle the shutdown.
    }

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

        DisconnectionController.Instance.loadingNewScene = true; // A flag that will prevent disconnection pop-ups from happening when we're in the middle of loading a new scene
        

        // Disable interactions when new scene is loading
        Runner.GetPlayerObject(Runner.LocalPlayer).GetComponentInChildren<PixelCrushers.DialogueSystem.ProximitySelector>().enabled = false;
        
        // This will disconnect from the current session and clean up networked objects.
        if (Runner != null)
        {
            await Runner.Shutdown();
        }        

        // Now load the new scene.
        SceneManager.LoadScene(newSceneName);        
    }

    public void OnDestroy()
    {
        Debug.Log("DISCONNECT: NetworkController destroyed");
        //SceneManager.LoadScene("Main Menu");
    }
}

