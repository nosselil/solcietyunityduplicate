using UnityEngine;

namespace Watermelon
{
    public interface IDropableItem
    {
        public bool IsRewarded { get; set; }
        public bool IsPicked { get; }
        public GameObject Object { get; }

        public DropData DropData { get; }

        public int DropAmount { get; }
        public DropableItemType DropType { get; }

        public void Initialise(DropData dropData, float availableToPickDelay = -1f, bool ignoreCollector = false);
        public void Pick(CharacterBehavior character, bool moveToPlayer = true);
        public void Drop();
        public void Throw(Vector3 position, AnimationCurve movemenHorizontalCurve, AnimationCurve movementVerticalCurve, float time);
        public bool IsPickable(CharacterBehavior character);
    }
}