using Fusion;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Collections.Unicode;

public class DisconnectionController : MonoBehaviour
{
    [SerializeField] GameObject disconnectionPopUp;

    public bool playerSpawnedInScene = false; // This will make sure that we're not displaying the disconnection pop-up before the player has even spawned. Basically we will be checking
                                              // whether the player still exists after the spawn event has occurred. If the player does not, then there must have been a disconnection event and we need to go back to the main menu.

    // NOTE: This does overlap with NetworkController.Instance.playerSpawned, but since that object gets destroyed if the network closes, we also need this class, which isn't linked
    // to network events. TODO: Could probably be refactored in some way though.

    public bool loadingNewScene = false; // Again, this kind of overlaps with the NetworkController, but we can at least be sure that this 

    public GameObject localPlayerGO;

    public static DisconnectionController Instance;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;

        loadingNewScene = false;

        StartCoroutine(PollLocalPlayerExistence());
        DisplayDisconnectionPopUp(false);
    }

    // Update is called once per frame
    void Update()
    {           
    }


    public void DisplayDisconnectionPopUp(bool active)
    {
        disconnectionPopUp.SetActive(active);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
    IEnumerator PollLocalPlayerExistence()
    {
        float pollInterval = 3f;

        yield return new WaitForSeconds(5f); // Wait a moment for the player instantiation to happen properly

        while (true)
        {
            //Debug.Log("DISCONNECT: Local player still exists: playerSpawnedInScene " + playerSpawnedInScene + " localPlayerGO " + localPlayerGO + ", loading new scene: " + loadingNewScene);

            if (playerSpawnedInScene && localPlayerGO == null && loadingNewScene)
            {
                DisplayDisconnectionPopUp(true);
                Debug.Log("DISCONNECT: Local player doesn't exist any more");
                yield break;
            }

            /*if (Runner != null)
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
            }*/

            yield return new WaitForSeconds(pollInterval);
        }
    }
}
