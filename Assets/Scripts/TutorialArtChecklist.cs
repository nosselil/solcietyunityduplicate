using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialArtChecklist : MonoBehaviour
{
    public Toggle toggleArtwork;
    public Toggle toggleNPC;
    public Toggle toggleButton1;
    public Toggle toggleButton2;

    public TextMeshProUGUI labelArtwork;
    public TextMeshProUGUI labelNPC;
    public TextMeshProUGUI labelButton1;
    public TextMeshProUGUI labelButton2;

    public Color completedColor = Color.gray;
    public Color normalColor = Color.white;

    private bool hasPressedTradeButton = false;
    private bool hasPressedArtworkSetButton = false;
    private bool hasInteractedArtwork = false;
    private bool hasTalkedToNPC = false;

    public CanvasGroup tutorialCanvasGroup; // Add this for fading
    private bool hasFaded = false;

    void Update()
    {
        UpdateLabelStyle(toggleArtwork, labelArtwork);
        UpdateLabelStyle(toggleNPC, labelNPC);
        UpdateLabelStyle(toggleButton1, labelButton1);
        UpdateLabelStyle(toggleButton2, labelButton2);

        // Fade out when all toggles are checked
        if (!hasFaded && AllTogglesOn())
        {
            hasFaded = true;
            StartCoroutine(FadeOutCanvas());
        }
    }

    bool AllTogglesOn()
    {
        return toggleArtwork.isOn && toggleNPC.isOn && toggleButton1.isOn && toggleButton2.isOn;
    }

    IEnumerator FadeOutCanvas()
    {
        float duration = 1f;
        float startAlpha = tutorialCanvasGroup.alpha;
        float time = 0;
        while (time < duration)
        {
            tutorialCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        tutorialCanvasGroup.alpha = 0;
        tutorialCanvasGroup.interactable = false;
        tutorialCanvasGroup.blocksRaycasts = false;
    }

    void UpdateLabelStyle(Toggle toggle, TextMeshProUGUI label)
    {
        if (toggle != null && label != null)
        {
            label.color = toggle.isOn ? completedColor : normalColor;
            label.fontStyle = toggle.isOn ? (label.fontStyle | FontStyles.Strikethrough) : (label.fontStyle & ~FontStyles.Strikethrough);
        }
    }

    // Call this from UnityEvent when artwork is interacted with
    public void OnInteractArtwork()
    {
        if (!hasInteractedArtwork)
        {
            hasInteractedArtwork = true;
            toggleArtwork.isOn = true;
        }
    }

    // Call this from UnityEvent when NPC is talked to
    public void OnTalkToNPC()
    {
        if (!hasTalkedToNPC)
        {
            hasTalkedToNPC = true;
            toggleNPC.isOn = true;
        }
    }

    // Call this from the UI Button for trade
    public void OnTradeButtonPressed()
    {
        if (!hasPressedTradeButton)
        {
            hasPressedTradeButton = true;
            toggleButton1.isOn = true;
        }
    }

    // Call this from the UI Button for artwork set
    public void OnArtworkSetButtonPressed()
    {
        if (!hasPressedArtworkSetButton)
        {
            hasPressedArtworkSetButton = true;
            toggleButton2.isOn = true;
        }
    }
} 