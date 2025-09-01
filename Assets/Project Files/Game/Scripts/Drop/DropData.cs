using Watermelon;

namespace Watermelon
{
    [System.Serializable]
    public class DropData
    {
        public DropableItemType dropType;

        public CurrencyType currencyType;

        public int amount;

        public DropData() { }

        public DropData Clone()
        {
            var data = new DropData();

            data.dropType = dropType;
            data.currencyType = currencyType;
            data.amount = amount;

            return data;
        }
    }
}