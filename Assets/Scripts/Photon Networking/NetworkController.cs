using UnityEngine;

public class NetworkController : MonoBehaviour
{
    public static NetworkController Instance;

    [HideInInspector] public bool localPlayerSpawned = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
