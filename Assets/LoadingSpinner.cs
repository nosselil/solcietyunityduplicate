using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingSpinner : MonoBehaviour
{
    [Header("Animation Settings")]
    public float rotationSpeed = 200f; // Degrees per second
    public float pulseSpeed = 2f; // Pulses per second
    public float pulseIntensity = 0.3f; // How much the alpha changes
    
    [Header("UI References")]
    public Image spinnerImage; // The rotating image
    public TextMeshProUGUI loadingText; // Optional loading text
    public CanvasGroup canvasGroup; // For fade in/out effects
    
    [Header("Text Animation")]
    public bool animateText = true;
    public string[] loadingMessages = { "Loading", "Loading.", "Loading..", "Loading..." };
    public float textChangeSpeed = 0.5f; // How often to change the text
    
    private bool isAnimating = false;
    private float textTimer = 0f;
    private int currentTextIndex = 0;
    
    void Awake()
    {
        // Get canvas group if not assigned
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        
        // Hide by default
        SetVisible(false);
    }
    
    void Update()
    {
        if (!isAnimating) return;
        
        // Rotate the spinner
        if (spinnerImage != null)
        {
            spinnerImage.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
        
        // Pulse the alpha (only for the spinner, not the background)
        if (spinnerImage != null)
        {
            Color spinnerColor = spinnerImage.color;
            float alpha = 1f + Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2) * pulseIntensity;
            spinnerColor.a = Mathf.Clamp01(alpha);
            spinnerImage.color = spinnerColor;
        }
        
        // Animate the text
        if (animateText && loadingText != null && loadingMessages.Length > 0)
        {
            textTimer += Time.deltaTime;
            if (textTimer >= textChangeSpeed)
            {
                textTimer = 0f;
                currentTextIndex = (currentTextIndex + 1) % loadingMessages.Length;
                loadingText.text = loadingMessages[currentTextIndex];
            }
        }
    }
    
    public void Show(string customMessage = null)
    {
        isAnimating = true;
        SetVisible(true);
        
        if (customMessage != null && loadingText != null)
        {
            loadingText.text = customMessage;
            animateText = false; // Disable text animation when using custom message
        }
        else if (loadingText != null && loadingMessages.Length > 0)
        {
            loadingText.text = loadingMessages[0];
            currentTextIndex = 0;
            textTimer = 0f;
            animateText = true;
        }
    }
    
    public void Hide()
    {
        isAnimating = false;
        SetVisible(false);
    }
    
    private void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
        else
        {
            gameObject.SetActive(visible);
        }
    }
    
    // Quick setup method for common use cases
    public void ShowWithMessage(string message)
    {
        Show(message);
    }
    
    // Method to show loading for a specific duration
    public void ShowForDuration(float duration, string message = null)
    {
        Show(message);
        Invoke(nameof(Hide), duration);
    }
} 