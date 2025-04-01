using UnityEngine;
using TMPro; // Important: Include the TextMeshPro namespace

namespace InsaneSystems.RTSStarterKit.Triggers
{
    public class damagecount: TriggerBase
    {
        public int triggerActivationCount = 0;

        // Use TMP_Text instead of Text
        public TMP_Text countText; // Or public TextMeshProUGUI countText; if it's a UI element

        protected override void ExecuteAction()
        {
            triggerActivationCount++;

            if (countText!= null)
            {
                countText.text = "Count: " + triggerActivationCount.ToString();
            }
            else
            {
                Debug.LogError("Count TextMeshPro UI element not assigned in the Inspector!");
            }

            Debug.Log("Trigger activated. Count: " + triggerActivationCount);
        }
    }
}