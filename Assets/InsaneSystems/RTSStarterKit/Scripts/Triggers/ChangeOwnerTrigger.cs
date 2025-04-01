using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Triggers
{
    public class ChangeOwnerTrigger : TriggerBase
    {
        // Public variable to track the number of units that enter
        public int unitsEnteredCount = 0;

        // Counter for the number of hearts removed
        private int heartsRemovedCount = 0;

        // Reference to the heart objects (assign in the Inspector)
        [SerializeField] private GameObject[] hearts;

        // Reference to the new object to enable after 6 hearts are removed
        [SerializeField] private GameObject newObjectToEnable;

        protected override void ExecuteAction()
        {
            
                unitsEnteredCount++; // Increment the counter

                // Remove a heart each time a unit enters
                if (heartsRemovedCount < hearts.Length)
                {
                    hearts[heartsRemovedCount].SetActive(false); // Disable the heart
                    heartsRemovedCount++; // Increment the heart removal counter
                }

                // Enable the new object after 6 hearts are removed
                if (heartsRemovedCount >= 6 && newObjectToEnable != null)
                {
                    newObjectToEnable.SetActive(true);
                }
            

            // Log the count to the console
            Debug.Log("Units Entered: " + unitsEnteredCount);
            Debug.Log("Hearts Removed: " + heartsRemovedCount);
        }
    }
}