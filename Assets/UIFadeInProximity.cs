using UnityEngine;

public class UIFadeProximity : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform targetObject; // Object near which the UI appears
    public float fadeDistance = 5f;

    [Header("UI Fade Settings")]
    public CanvasGroup uiElement;
    public float fadeSpeed = 2f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Use Trigger Instead")]
    public bool useTriggerInstead = false;

    private bool playerInsideTrigger = false;
    private float fadeProgress = 0f;

    void Update()
    {
        bool shouldFadeIn = false;

        if (useTriggerInstead)
        {
            shouldFadeIn = playerInsideTrigger;
        }

        float targetProgress = shouldFadeIn ? 1f : 0f;
        fadeProgress = Mathf.MoveTowards(fadeProgress, targetProgress, fadeSpeed * Time.deltaTime);
        uiElement.alpha = fadeCurve.Evaluate(fadeProgress);

        bool isVisible = uiElement.alpha >= 0.95f;
        uiElement.interactable = isVisible;
        uiElement.blocksRaycasts = isVisible;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!useTriggerInstead) return;

        if (other.CompareTag("Player"))
        {
            playerInsideTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!useTriggerInstead) return;

        if (other.CompareTag("Player"))
        {
            playerInsideTrigger = false;
        }
    }
}
