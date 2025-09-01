using UnityEngine;

namespace Watermelon
{
    // use ItemDropBehaviour if you DON'T NEED any special behaviour for the item
    // inherit this class to implement a unique behaviour 
    public abstract class BaseDropBehaviour : OppositeMover, IDropableItem
    {
        public bool IsRewarded { get; set; } = false;

        protected bool isPicked = false;
        public bool IsPicked => isPicked;

        public GameObject Object => gameObject;

        protected DropData dropData;
        public DropData DropData => dropData;

        public int DropAmount => dropData.amount;
        public DropableItemType DropType => dropData.dropType;

        protected float availableToPickDelay;
        protected float autoPickDelay;

        public abstract void Initialise(DropData dropData, float availableToPickDelay = -1f, bool ignoreCollector = false);
        public abstract void Pick(CharacterBehavior character, bool moveToPlayer = true);
        public abstract void Throw(Vector3 position, AnimationCurve movemenHorizontalCurve, AnimationCurve movementVerticalCurve, float time);
        public abstract void Drop();

        public virtual bool IsPickable(CharacterBehavior character)
        {
            return true;
        }
    }
}