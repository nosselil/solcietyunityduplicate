using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


namespace Knife.Portal
{
    public class PortalControlByKey : MonoBehaviour
    {
        [SerializeField] private KeyCode openKey;
        [SerializeField] private KeyCode closeKey;
        [SerializeField] private PortalTransition[] portalTransitions;

        public UnityEvent OnOpenPortal;

        private void Start()
        {
            if (SceneManager.GetActiveScene().name == "mainGalleryMultiplayer")
            {
                Debug.Log("GALLERY PORTAL: Open portal immediately");
                OpenPortal();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(openKey))
            {
                OpenPortal();
            }
            else if (Input.GetKeyDown(closeKey))
            {
                ClosePortal();
            }
        }

        // Correct: Public and no parameters
        public void OpenPortal()
        {
            Debug.Log("OpenPortal() method called!");
            foreach (var p in portalTransitions)
            {
                p.OpenPortal(); // Call the PortalTransition's OpenPortal
            }
            OnOpenPortal?.Invoke();
        }

        // Correct: Public and no parameters
        public void ClosePortal()
        {
            Debug.Log("ClosePortal() method called!");
            foreach (var p in portalTransitions)
            {
                p.ClosePortal(); // Call the PortalTransition's ClosePortal
            }
        }
    }
}