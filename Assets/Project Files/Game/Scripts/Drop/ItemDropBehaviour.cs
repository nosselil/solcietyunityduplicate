#pragma warning disable CS0414

using UnityEngine;

namespace Watermelon
{
    // basic drop item without any special behaviour
    // override this class to add extra data fields
    public class ItemDropBehaviour : BaseDropBehaviour
    {
        [SerializeField] Animator animator;
        [SerializeField] Collider triggerRef;
        [SerializeField] GameObject trailObject;

        private TweenCase[] throwTweenCases;

        public override void Initialise(DropData dropData, float availableToPickDelay = -1, bool ignoreCollector = false)
        {
            this.dropData = dropData;
            this.availableToPickDelay = availableToPickDelay;

            isPicked = false;

            if(animator != null) animator.enabled = false;
            if(trailObject != null) trailObject.SetActive(false);

            PlayerBehavior.SubscribeToOnCleared(ItemDisable);

            RegisterMovement();
        }

        public override void Drop()
        {
            if (animator != null) animator.enabled = true;
            triggerRef.enabled = true;
        }

        public override void Throw(Vector3 position, AnimationCurve movemenHorizontalCurve, AnimationCurve movementVerticalCurve, float time)
        {
            throwTweenCases = new TweenCase[2];

            if (trailObject != null) trailObject.SetActive(false);

            triggerRef.enabled = false;

            throwTweenCases[0] = transform.DOMoveXZ(position.x, position.z, time).SetCurveEasing(movemenHorizontalCurve);
            throwTweenCases[1] = transform.DOMoveY(position.y, time).SetCurveEasing(movementVerticalCurve).OnComplete(delegate
            {
                if (animator != null) animator.enabled = true;
                if (trailObject != null) trailObject.SetActive(true);

                Tween.DelayedCall(availableToPickDelay, () =>
                {
                    triggerRef.enabled = true;
                });
            });
        }

        private void OnTriggerEnter(Collider other)
        {
            CharacterBehavior character = other.GetComponent<CharacterBehavior>();

            if (character != null)
            {
                Pick(character);
            }
        }

        public override void Pick(CharacterBehavior character, bool moveToPlayer = true)
        {
            if (isPicked)
                return;

            isPicked = true;

            // Kill movement tweens
            if (!throwTweenCases.IsNullOrEmpty())
            {
                for (int i = 0; i < throwTweenCases.Length; i++)
                {
                    throwTweenCases[i].KillActive();
                }
            }

            if (animator != null) animator.enabled = false;
            triggerRef.enabled = false;

            if (moveToPlayer)
            {
                transform.DOMove(character.transform.position.SetY(0.625f), 0.2f).SetEasing(Ease.Type.SineIn).OnComplete(() =>
                {
                    ItemDisable();

                    if (dropData.dropType == DropableItemType.Money)
                    {
                        CurrencyController.Add(dropData.currencyType, DropAmount);
                        character.OnMoneyPickedUp();
                    
                        AudioController.PlaySound(CurrencyController.GetCurrency(dropData.currencyType).Data.PickUpSound);
                    } 
                    else if(dropData.dropType == DropableItemType.Heal)
                    {
                        character.Player.AddHealth(DropAmount);
                        AudioController.PlaySound(AudioController.AudioClips.heal, 0.35f);
                    }

                });
            }
            else
            {
                ItemDisable();
            }
        }

        public void ItemDisable()
        {
            PlayerBehavior.UnsubscribeFromOnCleared(ItemDisable);

            if(this != null)
            {
                if (gameObject != null)
                    gameObject.SetActive(false);
            }

            RemoveMover();
        }
    }
}