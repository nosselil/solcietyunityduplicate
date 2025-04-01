using UnityEngine;

public class ScreenSizeUI : MonoBehaviour
{
    void Update()
    {
        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta = Vector2.zero; // Reset size delta
    }
}