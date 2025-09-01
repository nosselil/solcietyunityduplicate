using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class CurrencyData
    {
        [SerializeField] bool displayAlways = false;
        public bool DisplayAlways => displayAlways;

        [SerializeField] GameObject dropPrefab;
        public GameObject DropPrefab => dropPrefab;

        [SerializeField] AudioClip pickUpSound;
        public AudioClip PickUpSound => pickUpSound;

        private Pool dropPool;
        public Pool DropPool => dropPool;

        public void Init(Currency currency)
        {
            dropPool = new Pool(dropPrefab, $"{currency.CurrencyType} Drop");
        }
    }
}