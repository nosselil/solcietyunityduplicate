using UnityEngine;

public class OpenURL : MonoBehaviour
{
    public string url = "https://theportal.to/lootbox"; // The URL you want to open

    // Public method to open the URL
    public void Open()
    {
        Application.OpenURL(url);
    }
}