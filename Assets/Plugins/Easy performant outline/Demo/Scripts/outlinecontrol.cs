using UnityEngine;

namespace EPOOutline
{
    public class ActivateOutlinableOnTrigger : MonoBehaviour
    {
        private Outlinable outlinable;

        private void Start()
        {
            // Get the Outlinable component
            outlinable = GetComponent<Outlinable>();

            // Initially disable the Outlinable component
            if (outlinable != null)
            {
                outlinable.enabled = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (outlinable != null)
                {
                    outlinable.enabled = true; 
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (outlinable != null)
                {
                    outlinable.enabled = false; 
                }
            }
        }
    }
}