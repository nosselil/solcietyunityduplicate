using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class BoosterData : AbstractData
    {
        [SerializeField] BoosterType boosterType;
        public BoosterType BoosterType => boosterType;

        // if true - user can set value and enable updateOnHit in the Level Editor. if false - the user have to give an id of the thing it wants to get ("Character", "Weapon", etc)
        [SerializeField] bool isNumerical;
        public bool IsNumerical => isNumerical;

        [SerializeField] Texture2D previewImage;
        public Texture2D PreviewImage => previewImage;
    }
}
