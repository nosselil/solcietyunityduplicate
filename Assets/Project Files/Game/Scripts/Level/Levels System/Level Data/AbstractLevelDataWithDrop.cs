using UnityEngine;

namespace Watermelon
{
    public abstract class AbstractLevelDataWithDrop : AbstractLevelData
    {
        [SerializeField] protected DropableItemType dropType;
        public DropableItemType DropType => dropType;

        [ShowIf("IsMoneyType")]
        [SerializeField] protected CurrencyType dropCurrencyType;
        public CurrencyType DropCurrencyType => dropCurrencyType;

        [SerializeField] protected int dropItemsCount = 1;
        public int DropItemsCount => dropItemsCount;

        [SerializeField] protected float dropItemValue;
        public float DropItemValue => dropItemValue;

        protected bool IsMoneyType()
        {
            return dropType == DropableItemType.Money;
        }
    }
}
