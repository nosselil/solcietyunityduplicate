using UnityEngine;
using UnityEngine.UI;

public class GameResourceUILabel : MonoBehaviour
{
    [SerializeField] private GameResource gameResourceData;
    private Text text;

    private void Awake()
    {
         text = GetComponent<Text>();
    }

    private void Update()
    {
        text.text = GameResourcesManager.instance.FindResourceData(gameResourceData).CurrentAmount.ToString();
    }
}
