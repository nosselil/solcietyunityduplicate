using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class ObstacleLevelData : AbstractLevelDataWithDrop
    {
        public int health;
        public int Health => health;

        [SerializeField] float boosterHeight;
        public float BoosterHeight => boosterHeight;

        public void SetHealth(int health)
        {
            this.health = health;
        }

        public void SetBoosterHeight(float boosterHeight)
        {
            this.boosterHeight = boosterHeight;
        }

        public void SetPosition(Vector3 position)
        {
            this.position = position;
        }

        public void SetDropType(DropableItemType dropType)
        {
            this.dropType = dropType;
        }

        public void SetDropCurrency(CurrencyType currencyType)
        {
            this.dropCurrencyType = currencyType;
        }

        public void SetDropItemValue(float dropItemValue)
        {
            this.dropItemValue = dropItemValue;
        }
    }
}
