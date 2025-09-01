using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class HorizontalGroup
    {
        private List<IPickableGameplayElement> pickables;
        private float posZ;

        public HorizontalGroup(IPickableGameplayElement gameplayElement)
        {
            pickables = new List<IPickableGameplayElement>();

            Add(gameplayElement);

            posZ = gameplayElement.PosZ;
        }

        private void Add(IPickableGameplayElement gameplayElement)
        {
            pickables.Add(gameplayElement);
            gameplayElement.OnPickedUp += OnPickedUp;
        }

        private void OnPickedUp(IPickableGameplayElement gameplayElement)
        {
            for(int i = 0; i < pickables.Count; i++)
            {
                IPickableGameplayElement pickable = pickables[i];

                pickable.CanBePickedUp = false;
                pickable.OnPickedUp -= OnPickedUp;
            }
        }

        public bool TryAdd(IPickableGameplayElement gameplayElement)
        {
            if(Mathf.Abs(gameplayElement.PosZ - posZ) < 0.2f)
            {
                Add(gameplayElement);

                return true;
            } 
            return false;
        }
        
    }
}
