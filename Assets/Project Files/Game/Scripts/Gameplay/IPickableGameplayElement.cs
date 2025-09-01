using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public interface IPickableGameplayElement
    {
        bool CanBePickedUp { get; set; }
        float PosZ { get; }
        public delegate void PickedUp(IPickableGameplayElement element);
        public event PickedUp OnPickedUp;
    }
}
