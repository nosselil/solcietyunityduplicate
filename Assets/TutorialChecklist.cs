using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro; // Add this if using TextMeshPro

public class TutorialChecklist : MonoBehaviour
{
    public Toggle toggleWalk;
    public Toggle toggleJump;
    public Toggle toggleRun;
    public Toggle toggleC;
    public Toggle toggleTalk;
    public CanvasGroup tutorialCanvasGroup; // <-- Add this
    public TextMeshProUGUI labelWalk;
    public TextMeshProUGUI labelJump;
    public TextMeshProUGUI labelRun;
    public TextMeshProUGUI labelC;
    public TextMeshProUGUI labelTalk;

    public Color completedColor = Color.gray; // Set in Inspector or pick your shade
    public Color normalColor = Color.white;   // Set in Inspector

    private bool hasWalked = false;
    private bool hasJumped = false;
    private bool hasRun = false;
    private bool hasPressedC = false;
    private bool hasFaded = false; // Prevent multiple fades

    void Update()
    {
        // 1. Walk detection (WASD or ZQSD)
        if (!hasWalked && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)
            || Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.Q)))
        {
            hasWalked = true;
            toggleWalk.isOn = true;
        }

        // 2. Jump detection (Space)
        if (!hasJumped && Input.GetKeyDown(KeyCode.Space))
        {
            hasJumped = true;
            toggleJump.isOn = true;
        }

        // 3. Run detection (Shift)
        if (!hasRun && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
        {
            hasRun = true;
            toggleRun.isOn = true;
        }

        // 4. Press C
        if (!hasPressedC && Input.GetKeyDown(KeyCode.C))
        {
            hasPressedC = true;
            toggleC.isOn = true;
        }

        // Update strikethrough for each label
        UpdateLabelStrikethrough(toggleWalk, labelWalk);
        UpdateLabelStrikethrough(toggleJump, labelJump);
        UpdateLabelStrikethrough(toggleRun, labelRun);
        UpdateLabelStrikethrough(toggleC, labelC);
        UpdateLabelStrikethrough(toggleTalk, labelTalk);

        // Update label color
        UpdateLabelStyle(toggleWalk, labelWalk);
        UpdateLabelStyle(toggleJump, labelJump);
        UpdateLabelStyle(toggleRun, labelRun);
        UpdateLabelStyle(toggleC, labelC);
        UpdateLabelStyle(toggleTalk, labelTalk);

        // Check if all toggles are on
        if (!hasFaded && AllTogglesOn())
        {
            hasFaded = true;
            StartCoroutine(FadeOutCanvas());
        }
    }

    bool AllTogglesOn()
    {
        return toggleWalk.isOn && toggleJump.isOn && toggleRun.isOn && toggleC.isOn && toggleTalk.isOn;
    }

    IEnumerator FadeOutCanvas()
    {
        float duration = 1f; // seconds
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

    void UpdateLabelStrikethrough(Toggle toggle, TextMeshProUGUI label)
    {
        if (toggle.isOn)
            label.fontStyle |= FontStyles.Strikethrough;
        else
            label.fontStyle &= ~FontStyles.Strikethrough;
    }

    void UpdateLabelStyle(Toggle toggle, TextMeshProUGUI label)
    {
        if (toggle.isOn)
            label.color = completedColor;
        else
            label.color = normalColor;
    }

    // 5. Call this from your NPC's OnExecute event
    public void OnTalkToNPC()
    {
        toggleTalk.isOn = true;
    }
}
