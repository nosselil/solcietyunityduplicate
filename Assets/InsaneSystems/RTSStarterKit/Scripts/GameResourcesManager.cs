using UnityEngine;

public class GameResourcesManager : MonoBehaviour
{
    public static GameResourcesManager instance;

    [SerializeField] private GameResource[] AllGameResources;

    [Header("Debugging/Visualize values in the inspector")]
    [SerializeField] private GameResourceData[] gameResourceDatas;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        gameResourceDatas = new GameResourceData[AllGameResources.Length];
        for (int i = 0; i < gameResourceDatas.Length; i++)
        {
            gameResourceDatas[i] = new GameResourceData(AllGameResources[i], 4200);
        }
    }

    public void AddResource(int amount, GameResource gameResource)
    {
        GameResourceData gameResourceData = FindResourceData(gameResource);
        gameResourceData.CurrentAmount += amount;
    }

    public void RemoveResource(int amount, GameResource gameResource)
    {
        GameResourceData gameResourceData = FindResourceData(gameResource);
        gameResourceData.CurrentAmount -= amount;
        if (gameResourceData.CurrentAmount < 0) gameResourceData.CurrentAmount = 0;
    }

    public GameResourceData FindResourceData(GameResource data)
    {
        for (int i = 0; i < gameResourceDatas.Length; i++)
        {
            if (gameResourceDatas[i].GameResource == data)
            {
                return gameResourceDatas[i];
            }
        }
        return null;
    }
}

[System.Serializable]
public class GameResourceData
{
    public GameResource GameResource;
    public int CurrentAmount = 0;

    public GameResourceData(GameResource gameResource,int amount)
    {
        this.GameResource = gameResource;
        this.CurrentAmount = amount;
    }
}