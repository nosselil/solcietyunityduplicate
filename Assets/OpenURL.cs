using UnityEngine;

public class OpenURL : MonoBehaviour
{
    public string url = "https://theportal.to/lootbox"; // The URL you want to open

    // Public method to open the URL
    public void Open()
    {
        ExternalUrlOpener.OpenExternalLink(url); // NOTE: The parameter isn't currently taken into account, but since there's only one link to open, it doesn't matter.
        //Application.OpenURL(url);
    }
}