using Avro;
using Newtonsoft.Json.Converters;
using PixelCrushers.DialogueSystem;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Starter
{
    /// <summary>
    /// Component that handles showing nicknames above player
    /// </summary>
    public class SpeechBubble : MonoBehaviour
    {
        public TextMeshProUGUI speechText;
        [SerializeField] private float speechBubbleDisplayTime;

        GameObject speechBubbleContainer;
        Animator animator;

        private Transform cameraTransform;

        Coroutine speechBubbleCoroutine;

        public void Start()
        {
            animator = GetComponent<Animator>();
            speechBubbleContainer = transform.GetChild(0).gameObject;
            speechBubbleContainer.gameObject.SetActive(false);
        }

        public void DisplaySpeechBubbleAnimation(string message)
        {            
            speechText.text = message;

            if (speechBubbleCoroutine != null)
                StopCoroutine(speechBubbleCoroutine);

            speechBubbleCoroutine = StartCoroutine(DisplayingSpeechBubbleAnimations());
        }


        IEnumerator DisplayingSpeechBubbleAnimations()
        {
            speechBubbleContainer.gameObject.SetActive(true);
            animator.Play("FadeIn", 0, 0);            
            yield return new WaitForSeconds(speechBubbleDisplayTime);
            animator.Play("FadeOut", 0, 0);
            yield return new WaitForSeconds(1f);
            speechBubbleContainer.gameObject.SetActive(false);
        }
        

        /*public void SetSpeechText(string message)
        {
            speechText.text = message;
        }*/

        private void Awake()
        {
            cameraTransform = Camera.main.transform;            
        }

        private void LateUpdate()
        {
            // Rotate speech bubble toward camera
            transform.rotation = cameraTransform.rotation;
        }
    }
}
