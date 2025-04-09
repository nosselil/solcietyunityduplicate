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
        loadingNewScene = true; // A flag that will prevent the player from interacting or moving

        // This will disconnect from the current session and clean up networked objects.
        if (Runner != null)
        {
            await Runner.Shutdown();
        }

        // Now load the new scene.
        SceneManager.LoadScene(newSceneName);

        // Optionally, after the scene has loaded (e.g., using a scene loaded callback),
        // you can initialize a new runner and pass in new StartGameArgs with SessionName set to newRoomName.
    }
}

